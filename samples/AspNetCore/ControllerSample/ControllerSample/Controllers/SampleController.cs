using Microsoft.AspNetCore.Http;
// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace ControllerSample.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Dapr;
    using Dapr.Client;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Sample showing Dapr integration with controller.
    /// </summary>
    [ApiController]
    public class SampleController : ControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleController"/> class.
        /// </summary>
        /// <param name="logger">A logger instance.</param>
        public SampleController(ILogger<SampleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// State store name.
        /// </summary>
        public const string StoreName = "statestore";
        private readonly ILogger<SampleController> _logger;

        /// <summary>
        /// Gets the account information as specified by the id.
        /// </summary>
        /// <param name="account">Account information for the id from Dapr state store.</param>
        /// <returns>Account information.</returns>
        [HttpGet("{account}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<Account> Get([FromState(StoreName)] StateEntry<Account> account)
        {
            if (account.Value is null)
            {
                return NotFound();
            }

            return account.Value;
        }

        /// <summary>
        /// Method for depositing to account as specified in transaction.
        /// </summary>
        /// <param name="transaction">Transaction info.</param>
        /// <param name="daprClient">State client to interact with Dapr runtime.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// "pubsub", the first parameter into the Topic attribute, is name of the default pub/sub configured by the Dapr CLI.
        [Topic("pubsub", "deposit")]
        [HttpPost("deposit")]
        public async Task<ActionResult<Account>> Deposit(Transaction transaction, [FromServices] DaprClient daprClient)
        {
            _logger.LogDebug("Enter deposit");
            var state = await daprClient.GetStateEntryAsync<Account>(StoreName, transaction.Id);
            state.Value ??= new Account() { Id = transaction.Id, };
            state.Value.Balance += transaction.Amount;
            await state.SaveAsync();
            return state.Value;
        }

        /// <summary>
        /// Method for withdrawing from account as specified in transaction.
        /// </summary>
        /// <param name="transaction">Transaction info.</param>
        /// <param name="daprClient">State client to interact with Dapr runtime.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        /// "pubsub", the first parameter into the Topic attribute, is name of the default pub/sub configured by the Dapr CLI.
        [Topic("pubsub", "withdraw")]
        [HttpPost("withdraw")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Account>> Withdraw(Transaction transaction, [FromServices] DaprClient daprClient)
        {
            _logger.LogDebug("Enter withdraw");
            var state = await daprClient.GetStateEntryAsync<Account>(StoreName, transaction.Id);

            if (state.Value == null)
            {
                return NotFound();
            }

            state.Value.Balance -= transaction.Amount;
            await state.SaveAsync();
            return state.Value;
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// Method for returning a BadRequest result which will cause Dapr sidecar to throw an RpcException.
        [HttpPost("throwException")]
        [ProducesDefaultResponseType]
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable SA1611 // Element parameters should be documented
        public async Task<ActionResult<Account>> ThrowException(Transaction transaction, [FromServices] DaprClient daprClient)
#pragma warning restore SA1611 // Element parameters should be documented
#pragma warning restore RCS1163 // Unused parameter.
        {
            Console.WriteLine("Enter ThrowException");
            var task = Task.Delay(10);
            await task;
            return BadRequest(new { statusCode = 400, message = "bad request" });
        }
    }
}
