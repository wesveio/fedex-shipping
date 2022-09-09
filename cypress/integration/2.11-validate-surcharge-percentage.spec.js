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
const surchargeFlatRate = 0
const surchargePercent = 30

describe('Modify SLA - Validate Surcharge Percentage in checkout', () => {
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

  it(` ${prefix} - Update Surcharge percentage`, updateRetry(3), () => {
    updateSLASettings(appSetting, surchargeFlatRate, surchargePercent)
  })

  it(
    `${prefix} - Validate Surcharge Percentage Changes`,
    updateRetry(3),
    () => {
      loadCalculateShippingAPI(data).then((response) => {
        validateCalculateShipping(response)
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === sla.FirstOvernight
        )

        const calculatePercentage = (amount * surchargePercent) / 100
        const calculateFlatRate = amount + surchargeFlatRate

        expect(filtershippingMethod[0].price.toFixed(2)).to.equal(
          (calculatePercentage + calculateFlatRate).toFixed(2)
        )
      })
    }
  )
})
