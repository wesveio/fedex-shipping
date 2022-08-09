import { updateRetry, loginViaCookies } from '../support/common/support'
import { appSetting } from '../support/outputvalidation'
import {
  graphql,
  saveAppSetting,
  validateSaveAppSettingResponse,
} from '../support/graphql_testcase'
import { FEDEX_SHIPPING_APP } from '../support/graphql_apps.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { updateSurchargeRateAndPercentage } from '../support/common.js'
import { loadCalculateShippingAPI } from '../support/api_testcase'
import sla from '../support/sla'

const prefix = 'Shipping Optimize'
let amount = ''
const surchargeFlatRate = 0
const surchargePercent = 30

describe('Modify SLA - Validate Surcharge Percentage in checkout', () => {
  loginViaCookies()

  it(`${prefix} - Update Surcharge Flat Rate`, updateRetry(3), () => {
    updateSurchargeRateAndPercentage().then((slaSettings) => {
      graphql(
        FEDEX_SHIPPING_APP,
        saveAppSetting(appSetting, slaSettings),
        validateSaveAppSettingResponse
      )
    })
  })

  it(`${prefix} - Verify shipping price`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === sla.FirstOvernight
      )

      amount = filtershippingMethod[0].price
    })
  })

  it(` ${prefix} - Update Surcharge percentage`, updateRetry(3), () => {
    updateSurchargeRateAndPercentage(surchargeFlatRate, surchargePercent).then(
      (slaSetting) => {
        graphql(
          FEDEX_SHIPPING_APP,
          saveAppSetting(appSetting, slaSetting),
          validateSaveAppSettingResponse
        )
      }
    )
  })

  it(
    `${prefix} - Validate Surcharge Percentage Changes`,
    updateRetry(3),
    () => {
      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === sla.FirstOvernight
        )

        const calculatePercentage = (amount * surchargePercent) / 100
        const calculateFlatRate = amount + surchargeFlatRate

        expect(filtershippingMethod[0].price).to.equal(
          calculatePercentage + calculateFlatRate
        )
      })
    }
  )
})
