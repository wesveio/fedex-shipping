import {
  preserveCookie,
  testSetup,
  updateRetry,
} from '../support/common/support.js'
import {
  getAppSettings,
  graphql,
  saveAppSetting,
  validateGetAppSettingsResponse,
  validateSaveAppSettingResponse,
} from '../support/fedex.graphql.js'
import {
  appSetting,
  smartPackingAccessKey,
} from '../support/fedex.outputvalidation.js'
import { data } from '../fixtures/shippingOptimizePayload.json'
import { calculateShippingAPI } from '../support/apis_endpoint'
import { FAIL_ON_STATUS_CODE } from '../support/common/constants.js'

const prefix = 'Shipping Optimize'

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`Get App Settings`, updateRetry(2), () => {
    graphql(getAppSettings(), (response) => {
      validateGetAppSettingsResponse(response)
      cy.getSettings(response.body)
    })
  })

  it.skip(`${prefix} - Generate access key`, updateRetry(2), () => {
    cy.visit('/admin/app/packing-optimization')
    cy.get('#accessKey')
      .should('be.visible')
      .clear()
      .type(smartPackingAccessKey)
    cy.contains('Save').click()
    cy.get('div[role="alert"] p').should('have.text', 'Successfully Saved')
  })

  it(
    `${prefix} - Select shipping optiomize type None and validate`,
    updateRetry(3),
    () => {
      appSetting.optimizeShippingType = 0
      cy.hideSla(false).then((sla) => {
        graphql(saveAppSetting(appSetting, sla), validateSaveAppSettingResponse)
      })
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

          expect(filtershippingMethod)
            .to.be.an('array')
            .and.to.have.lengthOf.above(0)
        })
      })
    }
  )

  it(
    `${prefix} - Select shipping optiomize type Pack All In One and validate`,
    updateRetry(3),
    () => {
      appSetting.optimizeShippingType = 1
      cy.hideSla(false).then((sla) => {
        graphql(saveAppSetting(appSetting, sla), validateSaveAppSettingResponse)
      })
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

          expect(filtershippingMethod)
            .to.be.an('array')
            .and.to.have.lengthOf.above(0)
        })
      })
    }
  )

  it.skip(
    `${prefix} - Select shipping optiomize type Smart Packing and validate`,
    updateRetry(3),
    () => {
      appSetting.optimizeShippingType = 2
      appSetting.packingAccessKey = smartPackingAccessKey
      cy.hideSla(false).then((sla) => {
        graphql(saveAppSetting(appSetting, sla), validateSaveAppSettingResponse)
      })
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

          expect(filtershippingMethod)
            .to.be.an('array')
            .and.to.have.lengthOf.above(0)
        })
      })
    }
  )

  preserveCookie()
})
