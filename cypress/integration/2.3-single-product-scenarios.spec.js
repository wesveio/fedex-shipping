import { testSetup, updateRetry } from '../support/common/support.js'
import { singleProduct, warehouseId } from '../support/outputvalidation.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { loadCalculateShippingAPI } from '../support/api_testcase.js'
import {
  graphql,
  verifyInventoryIsUnlimitedForFedexWareHouse,
  validateInventory,
} from '../support/graphql_testcase.js'
import { INVENTORY_GRAPHQL_APP } from '../support/graphql_apps.js'
import sla from '../support/sla.js'

const { prefix } = singleProduct
let amount = ''

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`${prefix} - For fedex docks, verify inventory is set to infinite`, () => {
    graphql(
      INVENTORY_GRAPHQL_APP,
      verifyInventoryIsUnlimitedForFedexWareHouse(
        warehouseId,
        data.items[0].id
      ),
      validateInventory
    )
  })

  it(`${prefix} - Verify single product shipping price`, updateRetry(3), () => {
    loadCalculateShippingAPI(data).then((response) => {
      const filtershippingMethod = response.body.filter(
        (b) => b.shippingMethod === sla.FirstOvernight
      )

      amount = filtershippingMethod[0].price
    })
  })

  it(
    `${prefix} - Verify product shipping price increase`,
    updateRetry(3),
    () => {
      data.items[0].quantity = 2
      loadCalculateShippingAPI(data).then((response) => {
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === sla.FirstOvernight
        )

        expect(filtershippingMethod[0].price).to.equal(amount * 2)
      })
    }
  )
})
