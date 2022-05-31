# FedEx Shipping
FedEx Dynamic Shipping Rates
FedEx App works in tandem with shipping-rates-provider to fetch for dynamic rates. This app acts as the middleware between FedEx and 

## Functionalities
- Fetch dynamic rates from FedEx

Test Calculate Shipping API

| Field | Value |
| --- | ---|
|URL|https://app.io.vtex.com/vtex.shipping-rates-provider/v0/{account}/{workspace}/shp-rates/calculate|
|METHOD|POST|
|Headers required|VtexIdclientAutCookie|

Request Body Examples
```json
{
    "items": [
        {
            "id": "880090",
            "quantity": 1,
            "unitPrice": 500.0,
            "modal": "",
            "unitDimension": {
                "weight": 10.0000,
                "height": 10.0000,
                "width": 10.0000,
                "length": 10.0000,
                "maxSumDimension": 0.0
            }
        }
    ],
    "origin": {
        "id": null,
        "zipCode": "33020",
        "country": "USA",
        "state": "FL",
        "city": "Hollywood",
        "coordinates": null,
        "residential": false
    },
    "destination": {
        "id": null,
        "zipCode": "00010002",
        "country": "USA",
        "state": null,
        "city": null,
        "coordinates": null,
        "residential": false
    },
    "shippingDateUTC": "2022-03-09T01:02:45.128577+00:00",
    "currency": null,
    "shippingRatesProvidersIds": [
        "vtexus.fedex-shipping"
    ]
}
```

```json
{
    "items": [
        {
            "id": "8999989",
            "quantity": 1,
            "modal": "",
            "unitPrice": 9.99,
            "unitDimension": {
                "weight": 1.5,
                "height": 30,
                "width": 10.7,
                "length": 20
            }
        }
    ],
    "origin": {
        "id":"",
        "street": "340 N 3rd St",
        "city": "Phoenix",
        "state": "AZ",
        "country": "USA",
        "zipCode": "85004",
        "residential": false,
        "coordinates": {
            "latitude": null,
            "longitude": null
        }
    },
    "destination": {
        "id":"",
        "street": "607 N Leroux St",
        "city": "Flagstaff",
        "state": "AZ",  
        "country": "USA",
        "zipCode": "86001",
        "residential": true,
        "coordinates": {
            "latitude": null,
            "longitude": null
        }
    },
    "shippingDateUTC": "2022-02-14T18:46:08.6986181+00:00",
    "currency": "USD",
    "shippingRatesProvidersIds": [
        "vtexus.fedex-shipping"
    ]
}
```

- Smart Packing
    - Ship Alone still ships alone
    - Anything else that can be packed together will be packed in the box dimensions