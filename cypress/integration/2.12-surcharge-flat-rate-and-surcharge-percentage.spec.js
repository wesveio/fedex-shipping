import { loginViaCookies, updateRetry } from '../support/common/support.js'
import {
  graphql,
  saveAppSetting,
  validateSaveAppSettingResponse,
} from '../support/graphql_testcase'
import { appSetting } from '../support/outputvalidation'
import { data } from '../fixtures/shippingRatePayload.json'
import { FEDEX_SHIPPING_APP } from '../support/graphql_apps.js'
import { updateSurchargeRateAndPercentage } from '../support/common.js'
import { loadCalculateShippingAPI } from '../support/api_testcase.js'
import sla from '../support/sla.js'

const prefix = 'Update SLA - Surcharge Flat Rate & Surcharge Percentage'
let amount = ''
const surchargeFlatRate = 10
const surchargePercent = 15

describe(`${prefix} Scenarios`, () => {
  loginViaCookies()

  it(
    `${prefix} - Update Surcharge Flat Rate and Surcharge Percentage`,
    updateRetry(3),
    () => {
      updateSurchargeRateAndPercentage().then((slaSettings) => {
        graphql(
          FEDEX_SHIPPING_APP,
          saveAppSetting(appSetting, slaSettings),
          validateSaveAppSettingResponse
        )
      })
    }
  )

  it(`${prefix} - Verify single product shipping price`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === sla.FedexHomeDelivery
      )

      amount = filtershippingMethod[0].price
    })
  })

  it(
    ` ${prefix} - Update Surcharge Flat Rate and Surcharge Percentage`,
    updateRetry(3),
    () => {
      updateSurchargeRateAndPercentage(
        surchargeFlatRate,
        surchargePercent
      ).then((slaSetting) => {
        graphql(
          FEDEX_SHIPPING_APP,
          saveAppSetting(appSetting, slaSetting),
          validateSaveAppSettingResponse
        )
      })
    }
  )

  it(`${prefix} - Validate Surcharge Changes`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === sla.FedexHomeDelivery
      )

      const calculatePercentage = (amount * surchargePercent) / 100
      const calculateFlatRate = amount + surchargeFlatRate

      expect(filtershippingMethod[0].price).to.equal(
        calculatePercentage + calculateFlatRate
      )
    })
  })
})
