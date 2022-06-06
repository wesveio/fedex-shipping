📢 Use this project, [contribute](https://github.com/vtex-apps/fedex-shipping) to it or open issues to help evolve it using [Store Discussion](https://github.com/vtex-apps/store-discussion).
# FedEx Shipping
<!-- DOCS-IGNORE:start -->
<!-- ALL-CONTRIBUTORS-BADGE:START - Do not remove or modify this section -->
[![All Contributors](https://img.shields.io/badge/all_contributors-0-orange.svg?style=flat-square)](#contributors-)
<!-- ALL-CONTRIBUTORS-BADGE:END -->
<!-- DOCS-IGNORE:end -->
The **FedEx Shipping** app is an integration with the VTEX [Dynamic Rates Hub](https://github.com/vtex/shipping-rates-provider) to perform real-time shipping cost calculations for the user in checkout. This app uses the FedEx API to calculate shipping costs.

## Features 🚚
- Fetch dynamic shipping rates from FedEx during checkout

## Installation ⏬
- Set Up a FedEx Developer Account
- Install the following two apps: `vtex.shipping-rates-provider` (the Dynamic Rates Hub app) and `vtexus.fedex-shipping` (this app) by running `vtex install vtex.shipping-rates-provider` and `vtex install vtexus.fedex-shipping` in your terminal, using the [VTEX IO CLI](https://developers.vtex.com/vtex-developer-docs/docs/vtex-io-documentation-vtex-io-cli-installation-and-command-reference).

## Configurations ⚙️

### How to *Access* Configuration
- In the VTEX Admin, `Search` for `FedEx Shipping`

### Standard Configurations 🔑
- `Meter Number`: This is the FedEx Meter Number
- `Account Number`: This is the FedEx Account Number
- `Credential Key`: This is the FedEx Credential Key
- `Credential Password`: This is the FedEx Credential Password
- `Is Live Toggle`: Toggles between account types. Ensure that the FedEx account values inputted above are reflective of the type here. Otherwise, it will not authenticate.
- `Ship To Residential`: Toggles the Shipping to Residential or Business. Certain SLAs will not be available for Residential or vise versa.
- `Optimize Shipping`: Toggles between **No Smart Packing**, **Pack All Into Largest Box**, or **Smart Packing**. See `Optimize Shipping` section for details

### Advance Configurations 
- `Configure Your Unit of Measurement`: This allows you to configure your unit of measurement for your items. Please set the `Weights` and `Dimensions` to a suitable unit of measurement. ***Incorrect units of measurement*** can cause rates to be drastically different than expected or even result in them not showing up.
- `Modify SLA`: This dropdown has a few settings
    - `Hide SLA`: If this is selected, the SLA will be hidden and not displayed. If all SLAs are hidden, none will be displayed.
    - `Surcharge Flat Rate`: Adds a flat surcharge to the rates. A negative value is not allowed.
        - > Example: If 2Day is \$150, and a \$20 `Flat Rate Surcharge` was addded, it would be $$ \$150 + \$20 = \$170$$
    - `Surcharge Percentage`: Adds a percentage surcharge to the rates. A negative value is allowed. To input a negative value, add the numeric values first, then add the negative sign.
        - > Example: If First Overnight is \$180, and a 30\% `Percent Surcharge` was added, it would be $$ \$180 + (\$180) * 30\% = \$234$$
    - Users can have both `Surcharge Flat Rate` and `Surcharge Percentage`. The two surcharges are added independently.
        - > Example: If Priority Overnight is \$135, and there was a \$10 `Flat Rate Surcharge` and a 15\% `Percentage Surcharge`, it would be $$ \$135 + \$10 + (\$135) * 30\% = \$185.5$$
- `Modal Mapping`: This dropdown has a few settings
    - `Ship Alone`: Items with this modal will be shipped indepently regardless of ***FedEx Handling Methods***
    - `FedEx Handling Method`: This is how FedEx will treat this modal. The options available are for FedEx Dangerous Goods handling. Please map the Modals accordingly with the desired ***FedEx Handling Method***. Select `None` if you want this modal to be treated with no special handling. The icon adjacent to the `FedEx Handling Method` dropdown indicates whether the item will be handled as a dangerous good or not. Items with the same `FedEx Handling Method` will be packed together.
        - > Example: If both `ELECTRONICS` and `WHITE_GOODS` both have Battery as a `FedEx Handling Type`, they will be grouped together and shipped together.
        - > Example: If `FURNITURE` is marked as `NONE`, it will be shipped with items that do not have a modal.

### Dock Configurations 🏬
- Connects docks to the app. If the Toggle is on, items can be shipped from this dock. 
- ⚠️⚠️⚠️**Ensure that the enabled docks have proper USA addresses**⚠️⚠️⚠️
- You can check dock settings here: `https://{workspace}--{account}.myvtex.com/admin/shipping-strategy/loading-docks/`
- Items coming from the same dock will be treated as if they are shipping together, unless an above setting prevents them from doing so.

### Optimize Shipping 📦
- There are 3 choices for optimizing shipping. Some may reduce shipping costs.
    - None: Treats every item as independent. Highest possible cost.
    - Pack All Into Largest Box: Packs all items into the largest item's box. Lowest possible cost since all items are combined into the dimensions of the largest item. However, this may be unreasonable as space in the box may already be preoccupied.
    - ⚠️***`BETA`*** Smart Packing: Given a list of boxes, we can dynamically pack items into the box, with respect to the space that is already consumed by other items in the box. This feature requires the [Packing Optimization App](https://github.com/vtex-apps/packing-optimization). Please check the [Packing Optimization App](https://github.com/vtex-apps/packing-optimization) for more information on how to use. Most optimal shipping cost.
- If `Smart Packing` is selected, an `Access Key` field will appear. Please input the Packing Optimization App's `Access Key` here. Press `Test Key` to see if the inputted key is valid. If `Smart Packing` is selected, the key must be valid, or else no rates will be returned.

### Things To Note ⚠️
> Remember to `Save` in the current settings tab before navigating to another settings tab. Otherwise, your changes will be lost.

> The FedEx dynamic rates can only be as good as the data inputted. Please correct the item's dimensions and weights and select the proper unit of measurement before using this app.

> Item length, width, and height are rounded up to the nearest whole number.

> A FedEx estimated delivery date will be shown at Order Management if an order has selected FedEx shipping.

> We currently do not support FedEx Freight shipping and international shipping. All shipments must not be over 100in<sup>3</sup>. All docks used must originate from the USA and destinations must also be in the USA.

> Services are limited to areas where FedEx operates. FedEx can not deliver to non-serviceable areas.

> All dock zipcodes must be valid

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

