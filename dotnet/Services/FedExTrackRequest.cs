using FedexShipping.Data;
using FedexShipping.Models;
using System;
using System.Threading.Tasks;
using TrackServiceReference;
using Vtex.Api.Context;

namespace FedexShipping.Services
{
    public class FedExTrackRequest : IFedExTrackRequest
    {
        private readonly IIOServiceContext _context;
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private MerchantSettings _merchantSettings;

        public FedExTrackRequest(IMerchantSettingsRepository merchantSettingsRepository, IIOServiceContext context)
        {
            this._merchantSettingsRepository = merchantSettingsRepository ??
                                            throw new ArgumentNullException(nameof(merchantSettingsRepository));

            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TrackReply> Track(string trackingNumber)
        {
            TrackReply reply = new TrackReply();
            
            try
            {
                this._merchantSettings = await _merchantSettingsRepository.GetMerchantSettings();
                TrackRequest request = CreateTrackRequest(trackingNumber);

                TrackPortTypeClient client;

                if (this._merchantSettings.IsLive)
                {
                    string remoteAddress = "https://ws.fedex.com:443/web-services";
                    client = new TrackPortTypeClient(TrackPortTypeClient.EndpointConfiguration.TrackServicePort, remoteAddress);
                }
                else
                {
                    client = new TrackPortTypeClient();
                }

                // Call the Track web service passing in a TrackRequest and returning a TrackReply
                trackResponse response = await client.trackAsync(request);
                reply = response.TrackReply;
                Console.WriteLine($"!reply! = {reply}");
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ShowTrackReply(reply);
                }

                ShowNotifications(reply);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("Track", null, 
                "Error:", ex,
                new[]
                {
                    ( "trackingNumber", trackingNumber ),
                });
            }

            return reply;
        }

        private TrackRequest CreateTrackRequest(string trackingNumber)
        {
            // Build the TrackRequest
            TrackRequest request = new TrackRequest();
            //
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
            request.TransactionDetail.CustomerTransactionId = "***Track Request***";  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //request.TransactionDetail.Localization
            
            request.Version = new VersionId();
            
            // Tracking information
            request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
            request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
            request.SelectionDetails[0].PackageIdentifier.Value = trackingNumber;
            //request.SelectionDetails[0].PackageIdentifier.Type

            // Date range is optional.
            // If omitted, set to false

            request.SelectionDetails[0].Destination = new Address();
            request.SelectionDetails[0].Destination.CountryCode = "US";
            request.SelectionDetails[0].Destination.PostalCode = "33327";

            // Include detailed scans is optional.
            // If omitted, set to false

            return request;
        }

        private void ShowTrackReply(TrackReply reply)
        {
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    Console.WriteLine("Tracking details:");
                    Console.WriteLine("**************************************");
                    ShowNotification(trackDetail.Notification);
                    Console.WriteLine("Tracking number: {0}", trackDetail.TrackingNumber);
                    Console.WriteLine("Tracking number unique identifier: {0}", trackDetail.TrackingNumberUniqueIdentifier);
                    Console.WriteLine("Track Status: {0} ({1})", trackDetail.StatusDetail.Description, trackDetail.StatusDetail.Code);
                    Console.WriteLine("Carrier code: {0}", trackDetail.CarrierCode);

                    if (trackDetail.OtherIdentifiers != null)
                    {
                        foreach (TrackOtherIdentifierDetail identifier in trackDetail.OtherIdentifiers)
                        {
                            Console.WriteLine("Other Identifier: {0} {1}", identifier.PackageIdentifier.Type, identifier.PackageIdentifier.Value);
                        }
                    }
                    if (trackDetail.Service != null)
                    {
                        Console.WriteLine("ServiceInfo: {0}", trackDetail.Service.Description);
                    }
                    if (trackDetail.PackageWeight != null)
                    {
                        Console.WriteLine("Package weight: {0} {1}", trackDetail.PackageWeight.Value, trackDetail.PackageWeight.Units);
                    }
                    if (trackDetail.ShipmentWeight != null)
                    {
                        Console.WriteLine("Shipment weight: {0} {1}", trackDetail.ShipmentWeight.Value, trackDetail.ShipmentWeight.Units);
                    }
                    if (trackDetail.Packaging != null)
                    {
                        Console.WriteLine("Packaging: {0}", trackDetail.Packaging);
                    }
                    Console.WriteLine("Package Sequence Number: {0}", trackDetail.PackageSequenceNumber);
                    Console.WriteLine("Package Count: {0} ", trackDetail.PackageCount);
                    if (trackDetail.DatesOrTimes != null)
                    {
                        foreach (TrackingDateOrTimestamp timestamp in trackDetail.DatesOrTimes)
                        {
                            Console.WriteLine("{0}: {1}", timestamp.Type, timestamp.DateOrTimestamp);
                        }
                    }
                    if (trackDetail.DestinationAddress != null)
                    {
                        Console.WriteLine("Destination: {0}, {1}", trackDetail.DestinationAddress.City, trackDetail.DestinationAddress.StateOrProvinceCode);
                    }
                    if (trackDetail.AvailableImages != null)
                    {
                        foreach (AvailableImagesDetail ImageDetail in trackDetail.AvailableImages)
                        {
                            Console.WriteLine("Image availability: {0}", ImageDetail.Type);
                        }
                    }
                    if (trackDetail.NotificationEventsAvailable != null)
                    {
                        foreach (NotificationEventType notificationEventType in trackDetail.NotificationEventsAvailable)
                        {
                            Console.WriteLine("NotificationEvent type : {0}", notificationEventType);
                        }
                    }

                    //Events
                    Console.WriteLine();
                    if (trackDetail.Events != null)
                    {
                        Console.WriteLine("Track Events:");
                        foreach (TrackEvent trackevent in trackDetail.Events)
                        {
                            if (trackevent.TimestampSpecified)
                            {
                                Console.WriteLine("Timestamp: {0}", trackevent.Timestamp);
                            }
                            Console.WriteLine("Event: {0} ({1})", trackevent.EventDescription, trackevent.EventType);
                            Console.WriteLine("***");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("**************************************");
                }
            }

        }
        private void ShowNotification(Notification notification)
        {
            Console.WriteLine(" Severity: {0}", notification.Severity);
            Console.WriteLine(" Code: {0}", notification.Code);
            Console.WriteLine(" Message: {0}", notification.Message);
            Console.WriteLine(" Source: {0}", notification.Source);
        }
        private void ShowNotifications(TrackReply reply)
        {
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                ShowNotification(notification);
            }
        }
    }
}
