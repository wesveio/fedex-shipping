import { updateRetry, loginViaCookies } from '../support/common/support'
import { appSetting } from '../support/outputvalidation'
import { data } from '../fixtures/shippingRatePayload.json'
import { updateSLASettings } from '../support/common.js'
import {
  loadCalculateShippingAPI,
  validateCalculateShipping,
} from '../support/api_testcase'
import sla from '../support/sla'

const prefix = 'Shipping Optimize'
let amount = ''
const surchargeFlatRate = 20
const surchargePercent = 0

describe('Modify SLA - Validate Surcharge Flat Rate in checkout', () => {
  loginViaCookies()

  it(`${prefix} - Update Surcharge Flat Rate`, updateRetry(3), () => {
    updateSLASettings(appSetting)
  })

  it(`${prefix} - Verify shipping price`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      validateCalculateShipping(response)
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === sla.FirstOvernight
      )

      amount = filtershippingMethod[0].price
    })
  })

  it(` ${prefix} - Update Surcharge Flat Rate`, updateRetry(3), () => {
    updateSLASettings(appSetting, surchargeFlatRate, surchargePercent)
  })

  it(`${prefix} - Validate Surcharge Flat Rate Changes`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      validateCalculateShipping(response)
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === sla.FirstOvernight
      )

      const calculateFlatRate = amount + surchargeFlatRate

      expect(filtershippingMethod[0].price).to.equal(calculateFlatRate)
    })
  })
})
