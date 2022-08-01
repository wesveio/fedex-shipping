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
import { appSetting } from '../support/fedex.outputvalidation.js'
import { data } from '../fixtures/shippingRatePayload.json'
import { calculateShippingAPI } from '../support/apis_endpoint'
import { FAIL_ON_STATUS_CODE } from '../support/common/constants.js'

const prefix = 'Ship To Residential'

describe(`${prefix} Scenarios`, () => {
  // Load test setup
  testSetup()

  it(`Get App Settings`, updateRetry(2), () => {
    graphql(getAppSettings(), (response) => {
      validateGetAppSettingsResponse(response)
      cy.getSettings(response.body)
    })
  })

  it(`${prefix} - Disable Ship to Residential`, updateRetry(3), () => {
    appSetting.residential = false
    cy.hideSla(false).then((sla) => {
      graphql(saveAppSetting(appSetting, sla), validateSaveAppSettingResponse)
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
          (b) => b.shippingMethod === 'FedEx Ground'
        )

        expect(filtershippingMethod)
          .to.be.an('array')
          .and.to.have.lengthOf.above(0)
      })
    })
  })

  it(`${prefix} - Enable Ship to Residential`, updateRetry(3), () => {
    appSetting.residential = true
    cy.hideSla(false).then((sla) => {
      graphql(saveAppSetting(appSetting, sla), validateSaveAppSettingResponse)
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
          (b) => b.shippingMethod === 'FedEx Home Delivery'
        )

        expect(filtershippingMethod)
          .to.be.an('array')
          .and.to.have.lengthOf.above(0)
      })
    })
  })

  preserveCookie()
})
