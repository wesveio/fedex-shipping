import { testSetup, updateRetry } from '../support/common/support.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShippingAPI } from '../support/apis_endpoint'
import { FAIL_ON_STATUS_CODE } from '../support/common/constants.js'

describe('Validate Shipping from different origins', () => {
  // Load test setup
  testSetup()

  it(
    'Use brazil shipment and verify we can able to place the order',
    updateRetry(3),
    () => {
      data.destination.zipCode = '06010'
      data.destination.country = 'ITA'

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
            (b) => b.shippingMethod === 'International Economy'
          )
          expect(filtershippingMethod)
            .to.be.an('array')
            .and.to.have.lengthOf.above(0)
        })
      })
    }
  )

  it(
    'Use Poland shipment and verify error message displays ',
    updateRetry(3),
    () => {
      data.destination.zipCode = '00-005'
      data.destination.country = 'PL'

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
          expect(response.status).to.have.equal(500)
        })
      })
    }
  )
})
