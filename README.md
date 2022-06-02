# FedEx Shipping
FedEx Dynamic Shipping Rates
FedEx App works in tandem with shipping-rates-provider to fetch for dynamic rates. This app acts as the middleware between FedEx and 

## Functionalities
---
- Fetch dynamic rates from FedEx

## Configurations And Set Up
---
### How to Set Up
- Set Up a FedEx Developer Account
- Install the app by running: `vtex.shipping-rates-provider` and `vtex install vtexus.fedex-shipping`

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
- `Configure Your Unit of Measurement`: This allows you to configure your unit of measurement for your items. Please set the `Weights` and `Dimensions` to a suitable unit of measurement. ***Incorrect units of measurement*** can cause rates to be drastically different than expected or even result in them not showing up.
- `Modify SLA`: This dropdown has a few settings
    - `Hide SLA`: If this is selected, the SLA will be hidden and not displayed. If all SLAs are hidden, none will be displayed.
    - `Surcharge Flat Rate`: Adds a flat surcharge to the rates. A negative value is not allowed.
        - > Example: If 2Day is \$150, and a \$20 `Flat Rate Surcharge` was addded, it would be $$ \$150 + \$20 = \$170$$
    - `Surcharge Percentage`: Adds a percentage surcharge to the rates. A negative value is allowed. To input a negative value, add the numeric values first, then add the negative sign.
        - > Example: If First Overnight is \$180, and a 30\% `Percent Surcharge` was added, it would be $$ \$180 + (\$180) * 30\% = \$234 $$
    - Users can have both `Surcharge Flat Rate` and `Surcharge Percentage`. The two surcharges are added independently.
        - > Example: If Priority Overnight is \$135, and there was a \$10 `Flat Rate Surcharge` and a 15\% `Percentage Surcharge`, it would be $$ \$135 + \$10 + (\$135) * 30\% = \$185.5 $$


---
### Dock Configurations
- Connects docks to the app. If the Toggle is on, items can be shipped from this dock. 
- ⚠️⚠️⚠️**Ensure that the enabled docks have proper USA addresses**⚠️⚠️⚠️
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

### Things To Note ⚠️
- Please `Save` in the current settings tab before navigating to another settings tab

- Smart Packing
    - Ship Alone still ships alone
    - Anything else that can be packed together will be packed in the box dimensions