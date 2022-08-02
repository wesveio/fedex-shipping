import { testSetup, updateRetry } from '../support/common/support.js'
import { singleProduct } from '../support/fedex.outputvalidation.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShippingAPI } from '../support/apis_endpoint'
import { FAIL_ON_STATUS_CODE } from '../support/common/constants.js'

const { prefix } = singleProduct
let amount = ''

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`${prefix} - Verify single product shipping price`, updateRetry(3), () => {
    cy.getVtexItems().then((vtex) => {
      cy.request({
        method: 'POST',
        url: calculateShippingAPI(vtex.account, Cypress.env('workspace').name),
        headers: { VtexIdclientAutCookie: vtex.userAuthCookieValue },
        ...FAIL_ON_STATUS_CODE,
        body: data,
      }).then((response) => {
        expect(response.status).to.have.equal(200)
        expect(response.body).to.be.an('array').and.to.have.lengthOf.above(0)
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === 'First Overnight'
        )

        amount = filtershippingMethod[0].price
      })
    })
  })

  it(
    `${prefix} - Verify product shipping price increase`,
    updateRetry(3),
    () => {
      data.items[0].quantity = 2
      cy.getVtexItems().then((vtex) => {
        cy.request({
          method: 'POST',
          url: calculateShippingAPI(
            vtex.account,
            Cypress.env('workspace').name
          ),
          headers: { VtexIdclientAutCookie: vtex.userAuthCookieValue },
          ...FAIL_ON_STATUS_CODE,
          body: data,
        }).then((response) => {
          expect(response.status).to.have.equal(200)
          expect(response.body).to.be.an('array').and.to.have.lengthOf.above(0)
          const filtershippingMethod = response.body.filter(
            (b) => b.shippingMethod === 'First Overnight'
          )

          expect(filtershippingMethod[0].price).to.equal(amount * 2)
        })
      })
    }
  )
})
