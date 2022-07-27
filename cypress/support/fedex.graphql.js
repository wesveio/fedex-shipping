import { FAIL_ON_STATUS_CODE } from './common/constants'

const path = require('path')

const config = Cypress.env()

// Constants
const { vtex } = config.base

export function graphql(getQuery, validateResponseFn = null) {
  const { query, queryVariables } = getQuery

  // Define constants
  const manifestFile = path.join('..', 'manifest.json')
  const APP_VERSION = manifestFile.version
  const APP_NAME = 'vtex.wish-list'
  const APP = `${APP_NAME}@${APP_VERSION}`
  const CUSTOM_URL = `${vtex.baseUrl}/_v/private/admin-graphql-ide/v0/${APP}`

  cy.request({
    method: 'POST',
    url: CUSTOM_URL,
    ...FAIL_ON_STATUS_CODE,
    body: {
      query,
      variables: queryVariables,
    },
  }).as('RESPONSE')

  if (validateResponseFn) {
    cy.get('@RESPONSE').then((response) => {
      expect(response.status).to.equal(200)
      expect(response.body.data).to.not.equal(null)
      validateResponseFn(response)
    })
  } else {
    return cy.get('@RESPONSE')
  }
}

export function getAppSettings() {
  return {
    query:
      'query' +
      '{  getAppSettings{userCredentialKey,userCredentialPassword,parentCredentialKey,parentCredentialPassword,clientDetailAccountNumber,clientDetailMeterNumber,isLive,residential,optimizeShippingType,unitWeight,unitDimension,packingAccessKey}}',
  }
}

export function getDocks() {
  return {
    query: 'query' + '{  getDocks{docksList{id,name,shippingRatesProviders}}}',
  }
}

export function saveAppSetting(appDatas, slaName, hide = false) {
  appDatas.slaSettings = [
    { sla: slaName, hidden: hide, surchargePercent: 0, surchargeFlatRate: 0 },
  ]
  const query =
    'mutation' +
    '($userCredentialKey: String, $userCredentialPassword: String, $parentCredentialKey: String, $parentCredentialPassword: String, $clientDetailMeterNumber: String, $clientDetailAccountNumber: String, $isLive: Boolean, $residential: Boolean,$optimizeShippingType: Int,$unitWeight: String,$unitDimension: String,$packingAccessKey: String,$slaSettings:[SlaSettingsInput])' +
    '{saveAppSetting(appSetting: {userCredentialKey:$userCredentialKey,userCredentialPassword:$userCredentialPassword,parentCredentialKey:$parentCredentialKey,parentCredentialPassword:$parentCredentialPassword,clientDetailMeterNumber:$clientDetailMeterNumber,clientDetailAccountNumber:$clientDetailAccountNumber,isLive:$isLive,residential:$residential,optimizeShippingType:$optimizeShippingType,unitWeight:$unitWeight,unitDimension:$unitDimension,packingAccessKey:$packingAccessKey,slaSettings:$slaSettings})}'

  return {
    query,
    queryVariables: appDatas,
  }
}

export function updateDockConnection(id, remove = false) {
  const query =
    'mutation' +
    '($dockId: String, $toRemove: Boolean)' +
    '{updateDockConnection(updateDock: {dockId:$dockId,toRemove:$toRemove})}'

  return {
    query,
    queryVariables: { dockId: id, toRemove: remove },
  }
}

export function validateGetAppSettingsResponse(response) {
  expect(response.body.data.getAppSettings).to.not.equal(null)
}

export function validateGetDockConnectionResponse(response) {
  expect(response.body.data.getDocks.docksList)
    .to.be.an('array')
    .and.to.have.lengthOf.above(0)
}

export function validateSaveAppSettingResponse(response) {
  expect(response.body.data.saveAppSetting).to.equal(true)
}

export function validateUpdateDockConnectionResponse(response) {
  expect(response.body.data.updateDockConnection).to.equal(true)
}
