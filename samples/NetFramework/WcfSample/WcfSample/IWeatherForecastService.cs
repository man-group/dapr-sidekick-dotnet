using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace WcfSample
{
    [ServiceContract]
    public interface IWeatherForecastService
    {
        [OperationContract]
        [WebGet]
        Message Health();

        [OperationContract]
        [WebGet]
        Message Metadata();

        [OperationContract]
        [WebGet]
        Message Metrics();

        [OperationContract]
        [WebGet]
        Message Status();

        [OperationContract]
        [WebGet]
        Message Weather();

        [OperationContract]
        [WebGet]
        Message WeatherSidecar();
    }
}
