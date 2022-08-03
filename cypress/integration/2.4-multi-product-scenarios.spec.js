import { testSetup, updateRetry } from '../support/common/support.js'
import { multiProduct } from '../support/outputvalidation.js'
import { data } from '../fixtures/multiProductPayload.json'
import { loadCalculateShippingAPI } from '../support/api_testcase.js'

const { prefix } = multiProduct
let amount = ''

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`${prefix} - Increase product quantity`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === 'First Overnight'
      )

      amount = filtershippingMethod[1].price
    })
  })

  it(
    `${prefix} - Verify product shipping price increase`,
    updateRetry(3),
    () => {
      data.items[1].quantity = 2
      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === 'First Overnight'
        )

        expect(parseFloat(filtershippingMethod[1].price.toFixed(2))).to.equal(
          parseFloat(amount) * 2
        )
      })
    }
  )

  it(
    `${prefix} - Verify product shipping price decreases`,
    updateRetry(3),
    () => {
      data.items[1].quantity = 1

      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === 'First Overnight'
        )

        expect(amount).to.equal(parseFloat(filtershippingMethod[1].price))
      })
    }
  )
})
