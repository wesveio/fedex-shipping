import { loadDocksAPI, calculateShippingAPI } from './apis.js'
import { updateRetry } from './common/support'
import { FAIL_ON_STATUS_CODE } from './common/constants'
import sla from './sla.js'

export function loadDocks() {
  it('Load all dock connection', updateRetry(3), () => {
    cy.getVtexItems().then((vtex) => {
      cy.getAPI(loadDocksAPI(vtex.baseUrl)).then((response) => {
        expect(response.status).to.have.equal(200)
      })
    })
  })
}

export function calculateShipping(data) {
  it('Calculate shipping', updateRetry(3), () => {
    cy.getVtexItems().then((vtex) => {
      cy.request({
        method: 'POST',
        url: calculateShippingAPI(vtex.account, Cypress.env('workspace').name),
        headers: { VtexIdclientAutCookie: vtex.userAuthCookieValue },
        ...FAIL_ON_STATUS_CODE,
        body: data,
      }).then((response) => {
        // If we receive statusCode 200 then our fedex shipping configuration is fine
        expect(response.status).to.have.equal(200)

        // If we receive empty array with valid payload then we can assume that fedex shipping site is down
        if (response.body.length === 0) {
          expect(response.body).to.be.an('array').and.to.have.lengthOf(0)
        } else {
          expect(response.body).to.be.an('array').and.to.have.lengthOf.above(0)
        }
      })
    })
  })
}

export function loadCalculateShippingAPI(data, validateResponseFn) {
  return cy.getVtexItems().then((vtex) => {
    cy.request({
      method: 'POST',
      url: calculateShippingAPI(vtex.account, Cypress.env('workspace').name),
      headers: { VtexIdclientAutCookie: vtex.userAuthCookieValue },
      ...FAIL_ON_STATUS_CODE,
      body: data,
    }).as('RESPONSE')

    if (validateResponseFn) {
      cy.get('@RESPONSE').then((response) => {
        expect(response.status).to.have.equal(200)
        expect(response.body).to.be.an('array').and.to.have.lengthOf.above(0)
        validateResponseFn(response)
      })
    } else {
      return cy.get('@RESPONSE')
    }
  })
}

export function validateCalculateShipping(response) {
  expect(response.body).to.be.an('array').and.to.have.lengthOf.above(0)
}

export function validateInternationEconomyShipping(response) {
  const filtershippingMethod = response.body.filter(
    (b) => b.shippingMethod === sla.InternationalEconomy
  )

  expect(filtershippingMethod).to.be.an('array').and.to.have.lengthOf.above(0)
}
