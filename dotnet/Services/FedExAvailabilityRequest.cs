using FedExAvailabilityServiceReference;
using FedexShipping.Data;
using FedexShipping.Models;
using System;
using System.Threading.Tasks;
using Vtex.Api.Context;

namespace FedexShipping.Services
{
    public class FedExAvailabilityRequest : IFedExAvailabilityRequest
    {
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private MerchantSettings _merchantSettings;
        private readonly IIOServiceContext _context;

        public FedExAvailabilityRequest(IMerchantSettingsRepository merchantSettingsRepository, IIOServiceContext context)
        {
            this._merchantSettingsRepository = merchantSettingsRepository ??
                                            throw new ArgumentNullException(nameof(merchantSettingsRepository));

            this._context = context 
                ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<serviceAvailabilityResponse> GetAvailability()
        {
            serviceAvailabilityResponse reply = new serviceAvailabilityResponse();

            try
            {
                this._merchantSettings = await _merchantSettingsRepository.GetMerchantSettings();
                ServiceAvailabilityRequest request = CreateServiceAvailabilityRequest();
                ValidationAvailabilityAndCommitmentPortTypeClient client;
                string remoteAddress = string.Empty;
            
                if (this._merchantSettings.IsLive)
                {
                    remoteAddress = "https://ws.fedex.com/web-services/ValidationAvailabilityAndCommitment";
                    client = new ValidationAvailabilityAndCommitmentPortTypeClient(ValidationAvailabilityAndCommitmentPortTypeClient.EndpointConfiguration.ValidationAvailabilityAndCommitmentServicePort, remoteAddress);
                }
                else
                {
                    remoteAddress = "https://wsbeta.fedex.com/web-services/ValidationAvailabilityAndCommitment";
                    client = new ValidationAvailabilityAndCommitmentPortTypeClient(ValidationAvailabilityAndCommitmentPortTypeClient.EndpointConfiguration.ValidationAvailabilityAndCommitmentServicePort, remoteAddress);
                }

                reply = await client.serviceAvailabilityAsync(request);
                ShowServiceAvailabilityReply(reply);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetAvailability", null, "Error:", ex);
            }

            return reply;
        }

        private ServiceAvailabilityRequest CreateServiceAvailabilityRequest()
        {
            // Build the ServiceAvailabilityRequest
            ServiceAvailabilityRequest request = new ServiceAvailabilityRequest();

            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = this._merchantSettings.UserCredentialKey;
            request.WebAuthenticationDetail.UserCredential.Password = this._merchantSettings.UserCredentialPassword;
            request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.ParentCredential.Key = this._merchantSettings.ParentCredentialKey;
            request.WebAuthenticationDetail.ParentCredential.Password = this._merchantSettings.ParentCredentialPassword;

            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = this._merchantSettings.ClientDetailAccountNumber;
            request.ClientDetail.MeterNumber = this._merchantSettings.ClientDetailMeterNumber;

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Service Availability Request using VC#***"; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            
            request.Version = new VersionId(); // Creates the Version element with all child elements populated from the wsdl
            
            request.Origin = new Address(); // Origin information
            request.Origin.PostalCode = "77510";
            request.Origin.CountryCode = "US";
            
            request.Destination = new Address(); // Destination information
            request.Destination.PostalCode = "38017";
            request.Destination.CountryCode = "US";

            request.ShipDate = DateTime.Now; // Shipping date and time
            request.Packaging = "YOUR_PACKAGING"; // Packaging type FEDEX_BOX, FEDEX_PAK, FEDEX_TUBE, YOUR_PACKAGING, ...

            return request;
        }

        private void ShowServiceAvailabilityReply(serviceAvailabilityResponse reply)
        {
            Console.WriteLine("ServiceAvailabilityReply details:");
            Console.WriteLine("*********");
            foreach (ServiceAvailabilityOption option in reply.ServiceAvailabilityReply.Options)
            {
                // if(option.ServiceSpecified)
                Console.WriteLine("Service Type : " + option.Service.ToString());
                if (option.DeliveryDateSpecified) Console.WriteLine("Delivery Date : " + option.DeliveryDate.ToShortDateString());
                if (option.DeliveryDaySpecified) Console.WriteLine("Delivery Day : " + option.DeliveryDay.ToString());
                if (option.DestinationStationId != null) Console.WriteLine("Destination Station ID : " + option.DestinationStationId);
                if (option.DestinationAirportId != null) Console.WriteLine("Destination Airport ID : " + option.DestinationAirportId);
                if (option.TransitTimeSpecified) Console.WriteLine("Transit Time : " + option.TransitTime);
                Console.WriteLine();
            }
            Console.WriteLine("*********");
        }
    }
}
