import selectors from '../support/common/selectors.js'
import { testSetup, updateRetry } from '../support/common/support.js'
import { singleProduct } from '../support/fedex.outputvalidation.js'
import fedexSelectors from '../support/fedex.selectors.js'
import { HomeFedex } from '../support/sla'

const { productName, prefix } = singleProduct
const { ShippingName, ShippingDeliveryTime } = HomeFedex
let amount = ''

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`${prefix} - Adding Product to Cart`, updateRetry(1), () => {
    // Search the product
    cy.searchProduct(productName)
    // Add product to cart
    cy.addProduct(productName, {
      proceedtoCheckout: true,
    })
  })

  it(`${prefix} - Increase product quantity`, updateRetry(3), () => {
    cy.get(fedexSelectors.ShippingSelectContainer).should('be.visible')
    cy.get(fedexSelectors.ShippingSelectContainerSelect).select(ShippingName)
    cy.get(fedexSelectors.CurrentShippingPrice)
      .should('be.visible')
      .invoke('text')
      .then((text) => {
        amount = text.split(' ')
        cy.get(selectors.ProductQuantityInCheckout(1), { timeout: 15000 })
          .should('be.visible')
          .should('not.be.disabled')
          .focus()
          .type(`{backspace}${2}{enter}`)
        cy.get(fedexSelectors.CurrentShippingSLA).contains(ShippingDeliveryTime)
      })
  })

  it(
    `${prefix} - Verify product shipping price increase`,
    updateRetry(3),
    () => {
      // eslint-disable-next-line cypress/no-unnecessary-waiting
      cy.wait(10000)
      cy.get(fedexSelectors.ShippingSummary, { timeout: 10000 })
        .should('be.visible')
        .contains('Shipping', { timeout: 6000 })
        .siblings('td.monetary', { timeout: 3000 })
        .should('have.text', `$ ${parseFloat(amount[1]) * 2}`)
    }
  )
})
