# FedEx Shipping
FedEx Dynamic Shipping Rates
FedEx App works in tandem with shipping-rates-provider to fetch for dynamic rates. This app acts as the middleware between FedEx and 

## Functionalities
---
- Fetch dynamic rates from FedEx

## Configurations
---
### Standard Configurations
- `Meter Number`: This is the FedEx Meter Number
- `Account Number`: This is the FedEx Account Number
- `Credential Key`: This is the FedEx Credential Key
- `Credential Password`: This is the FedEx Credential Password
- `Is Live Toggle`: Toggles between account types. Ensure that the FedEx account values inputted above are reflective of the type here. Otherwise, it will not authenticate.
- `Ship To Residential`: Toggles the Shipping to Residential or Business. Certain SLAs will not be available for Residential or vise versa.
- `Optimize Shipping`: Toggles between **No Smart Packing**, **Pack All Into Largest Box**, or **Smart Packing**. See `Optimize Shipping` section for details
---
### Advance Configurations

---
### Dock Configurations
- Connects docks to the app. If the Toggle is on, items can be shipped from this dock. 
- ⚠️⚠️⚠️**Ensure that the enabled docks have proper addresses**⚠️⚠️⚠️
- You can check dock settings here: `https://{workspace}--{account}.myvtex.com/admin/shipping-strategy/loading-docks/`

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
            "groupId": null,
            "unitPrice": 500.0,
            "modal": "",
            "unitDimension": {
                "weight": 10.00,
                "height": 10,
                "width": 10,
                "length": 10
            }
        }
    ],
    "origin": {
        "zipCode": "33020",
        "country": "USA",
        "state": "FL",
        "city": "Hollywood",
        "coordinates": null,
        "residential": false
    },
    "destination": {
        "zipCode": "00010002",
        "country": "USA",
        "state": null,
        "city": null,
        "coordinates": null,
        "residential": false
    },
    "shippingDateUTC": "2022-05-31T01:02:45.128577+00:00",
    "currency": null
}
```



- Smart Packing
    - Ship Alone still ships alone
    - Anything else that can be packed together will be packed in the box dimensions