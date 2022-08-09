import { loginViaCookies, updateRetry } from '../support/common/support.js'
import { appSetting } from '../support/outputvalidation'
import { data } from '../fixtures/shippingRatePayload.json'
import { updateSLASettings } from '../support/common.js'
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
      updateSLASettings(appSetting)
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
      updateSLASettings(appSetting, surchargeFlatRate, surchargePercent)
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
