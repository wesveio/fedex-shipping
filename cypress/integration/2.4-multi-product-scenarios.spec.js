import { testSetup, updateRetry } from '../support/common/support.js'
import { multiProduct } from '../support/fedex.outputvalidation.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShippingAPI } from '../support/apis_endpoint'
import { FAIL_ON_STATUS_CODE } from '../support/common/constants.js'

const { prefix } = multiProduct
let amount = ''

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`${prefix} - Increase product quantity`, updateRetry(3), () => {
    data.items = []
    data.items.push({
      id: '880350',
      quantity: 1,
      groupId: null,
      unitPrice: 94.0,
      modal: '',
      unitDimension: {
        weight: 10,
        height: 10,
        width: 10,
        length: 10,
      },
    })
    data.items.push({
      id: '880330',
      quantity: 1,
      groupId: null,
      unitPrice: 94.0,
      modal: '',
      unitDimension: {
        weight: 10,
        height: 10,
        width: 10,
        length: 10,
      },
    })
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

        amount = filtershippingMethod[1].price
      })
    })
  })

  it(
    `${prefix} - Verify product shipping price increase`,
    updateRetry(3),
    () => {
      data.items[1].quantity = 2
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

          expect(parseFloat(filtershippingMethod[1].price.toFixed(2))).to.equal(
            parseFloat(amount) * 2
          )
        })
      })
    }
  )

  it(
    `${prefix} - Verify product shipping price decreases`,
    updateRetry(3),
    () => {
      data.items[1].quantity = 1
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

          expect(amount).to.equal(parseFloat(filtershippingMethod[1].price))
        })
      })
    }
  )
})
