// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

namespace DaprDemoActor
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Dapr.Actors.Runtime;
    using IDemoActorInterface;
    using Microsoft.Extensions.Logging;

    // The following example showcases a few features of Actors
    //
    // Every actor should inherit from the Actor type, and must implement one or more actor interfaces.
    // In this case the actor interfaces are IDemoActor and IBankActor.
    //
    // For Actors to use Reminders, it must derive from IRemindable.
    // If you don't intend to use Reminder feature, you can skip implementing IRemindable and reminder
    // specific methods which are shown in the code below.
    public class DemoActor : Actor, IDemoActor, IBankActor, IRemindable
    {
        private const string StateName = "my_data";

        private readonly BankService _bank;
        private readonly ILogger<DemoActor> _logger;

        public DemoActor(ActorHost host, BankService bank, ILogger<DemoActor> logger)
            : base(host)
        {
            // BankService is provided by dependency injection.
            // See Program.cs
            _bank = bank;
            _logger = logger;
        }

        public async Task SaveData(MyData data)
        {
            _logger.LogInformation("This is Actor id {ActorId} with data {ActorData}.", Id, data);

            // Set State using StateManager, state is saved after the method execution.
            await StateManager.SetStateAsync<MyData>(StateName, data);
        }

        public Task<MyData> GetData()
        {
            // Get state using StateManager.
            return StateManager.GetStateAsync<MyData>(StateName);
        }

        public Task TestThrowException()
        {
            throw new NotImplementedException();
        }

        public Task TestNoArgumentNoReturnType()
        {
            return Task.CompletedTask;
        }

        public async Task RegisterReminder()
        {
            await RegisterReminderAsync("TestReminder", null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }

        public Task UnregisterReminder()
        {
            return UnregisterReminderAsync("TestReminder");
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            // This method is invoked when an actor reminder is fired.
            var actorState = await StateManager.GetStateAsync<MyData>(StateName);
            actorState.PropertyB = $"Reminder triggered at '{DateTime.Now:yyyy-MM-ddTHH:mm:ss}'";
            await StateManager.SetStateAsync<MyData>(StateName, actorState);
        }

        private class TimerParams
        {
            public int IntParam { get; set; }

            public string StringParam { get; set; }
        }

        /// <inheritdoc/>
        public Task RegisterTimer()
        {
            var timerParams = new TimerParams
            {
                IntParam = 100,
                StringParam = "timer test",
            };

            var serializedTimerParams = JsonSerializer.SerializeToUtf8Bytes(timerParams);
            return RegisterTimerAsync("TestTimer", nameof(this.TimerCallback), serializedTimerParams, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        }

        public Task UnregisterTimer()
        {
            return UnregisterTimerAsync("TestTimer");
        }

        /// <summary>
        /// This method is called when the timer is triggered based on its registration.
        /// It updates the PropertyA value.
        /// </summary>
        /// <param name="data">Timer input data.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task TimerCallback(byte[] data)
        {
            var state = await StateManager.GetStateAsync<MyData>(StateName);
            state.PropertyA = $"Timer triggered at '{DateTime.Now:yyyyy-MM-ddTHH:mm:s}'";
            await StateManager.SetStateAsync<MyData>(StateName, state);
            var timerParams = JsonSerializer.Deserialize<TimerParams>(data);
            _logger.LogInformation("Timer parameter1: {TimerParameter1}", timerParams.IntParam);
            _logger.LogInformation("Timer parameter2: {TimerParameter2}", timerParams.StringParam);
        }

        public async Task<AccountBalance> GetAccountBalance()
        {
            var starting = new AccountBalance()
            {
                AccountId = Id.GetId(),
                Balance = 100m, // Start new accounts with 100, we're pretty generous.
            };

            var balance = await StateManager.GetOrAddStateAsync<AccountBalance>("balance", starting);
            return balance;
        }

        public async Task Withdraw(WithdrawRequest withdraw)
        {
            var starting = new AccountBalance()
            {
                AccountId = Id.GetId(),
                Balance = 100m, // Start new accounts with 100, we're pretty generous.
            };

            var balance = await StateManager.GetOrAddStateAsync<AccountBalance>("balance", starting);

            // Throws Overdraft exception if the account doesn't have enough money.
            var updated = _bank.Withdraw(balance.Balance, withdraw.Amount);

            balance.Balance = updated;
            await StateManager.SetStateAsync("balance", balance);
        }

        // This method is called whenever an actor is activated.
        // An actor is activated the first time any of its methods are invoked.
        protected override Task OnActivateAsync()
        {
            // Provides opportunity to perform some optional setup.
            return Task.CompletedTask;
        }

        // This method is called whenever an actor is deactivated after a period of inactivity.
        protected override Task OnDeactivateAsync()
        {
            // Provides Opportunity to perform optional cleanup.
            return Task.CompletedTask;
        }
    }
}
