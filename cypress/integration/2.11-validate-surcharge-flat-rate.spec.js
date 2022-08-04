import {
  preserveCookie,
  updateRetry,
  testSetup,
} from '../support/common/support'
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

const prefix = 'Shipping Optimize'
let amount = ''
const surchargeFlatRate = 20

describe('Modify SLA - Validate Surcharge Flat Rate in checkout', () => {
  testSetup()

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
        (b) => b.shippingMethod === 'First Overnight'
      )

      amount = filtershippingMethod[0].price
    })
  })

  it(` ${prefix} - Update Surcharge Flat Rate`, updateRetry(3), () => {
    updateSurchargeRateAndPercentage(surchargeFlatRate).then((slaSettings) => {
      graphql(
        FEDEX_SHIPPING_APP,
        saveAppSetting(appSetting, slaSettings),
        validateSaveAppSettingResponse
      )
    })
  })

  it(`${prefix} - Validate Surcharge Flat Rate Changes`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === 'First Overnight'
      )

      const calculateFlatRate = amount + surchargeFlatRate

      expect(filtershippingMethod[0].price).to.equal(calculateFlatRate)
      amount = filtershippingMethod[0].price
    })
  })

  preserveCookie()
})
