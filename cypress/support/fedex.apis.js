import { loadDocksAPI, calculateShippingAPI } from './apis.endpoint'
import { updateRetry } from './common/support'
import { FAIL_ON_STATUS_CODE } from './common/constants'

export function loadDocks() {
  it('Load all dock connection', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
    cy.getVtexItems().then((vtex) => {
      cy.getAPI(loadDocksAPI(vtex.baseUrl)).then((response) => {
        expect(response.status).to.have.equal(200)
      })
    })
  })
}

export function calculateShipping(data) {
  it('Calculate shipping', updateRetry(3), () => {
    cy.addDelayBetweenRetries(2000)
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
      })
    })
  })
}
