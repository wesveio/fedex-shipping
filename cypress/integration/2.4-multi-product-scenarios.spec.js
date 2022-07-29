import selectors from '../support/common/selectors.js'
import { testSetup, updateRetry } from '../support/common/support.js'
import { multiProduct } from '../support/fedex.outputvalidation.js'
import { HomeFedex } from '../support/sla'
import fedexSelectors from '../support/fedex.selectors.js'

const { productName1, productName2, prefix } = multiProduct
const { ShippingName, ShippingDeliveryTime } = HomeFedex
let amount = ''

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`${prefix} - Adding Product to Cart`, updateRetry(1), () => {
    // Search the product
    cy.searchProduct(productName1)
    // Add product to cart
    cy.addProduct(productName1, {
      proceedtoCheckout: false,
    })
    // Search the product
    cy.searchProduct(productName2)
    // Add product to cart
    cy.addProduct(productName2, {
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
        cy.get(selectors.ProductQuantityInCheckout(2), { timeout: 15000 })
          .should('be.visible')
          .should('not.be.disabled')
          .focus()
          .type(`{backspace}${3}{enter}`)
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
        .should('have.text', `$ ${(parseFloat(amount[1]) / 2) * 4}`)
    }
  )

  it(
    `${prefix} - Verify product shipping price decreases`,
    updateRetry(3),
    () => {
      cy.get(selectors.ProductQuantityInCheckout(2), { timeout: 15000 })
        .should('be.visible')
        .should('not.be.disabled')
        .focus()
        .type(`{backspace}${2}{enter}`)
      // eslint-disable-next-line cypress/no-unnecessary-waiting
      cy.wait(10000)
      cy.get(fedexSelectors.ShippingSummary, { timeout: 10000 })
        .should('be.visible')
        .contains('Shipping', { timeout: 6000 })
        .siblings('td.monetary', { timeout: 3000 })
        .should('have.text', `$ ${(parseFloat(amount[1]) / 2) * 3}`)
    }
  )
})
