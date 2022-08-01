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
  const APP_NAME = 'vtex.fedex-shiping'
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

/* 
vtexus.fedex-shipping and vtex.packing-optimization uses same graphql name
eg: getAppSettings(), saveAppSetting()

If we run getAppSettings() or saveAppSetting(). It throws Error
Invalid GraphQL query. Multiple app dependencies have defined \"getAppSettings\". 
To fix this ambiguity you can use the @context directive to specify the app you need this data from


To solve this error use @context(provider: "vtexus.fedex-shipping")
*/

export function getAppSettings() {
  return {
    query:
      'query' +
      '{ getAppSettings @context(provider: "vtexus.fedex-shipping")' +
      '{userCredentialKey,userCredentialPassword,parentCredentialKey,parentCredentialPassword,clientDetailAccountNumber,clientDetailMeterNumber,isLive,residential,optimizeShippingType,unitWeight,unitDimension,packingAccessKey,slaSettings{sla,hidden,surchargePercent,surchargeFlatRate}}}',
  }
}

export function getDocks() {
  return {
    query: 'query' + '{  getDocks{docksList{id,name,shippingRatesProviders}}}',
  }
}

export function saveAppSetting(appDatas, allSla, slaName, hide = false) {
  if (slaName) {
    appDatas.slaSettings = [
      { sla: slaName, hidden: hide, surchargePercent: 0, surchargeFlatRate: 0 },
    ]
  }
  if (allSla) {
    appDatas.slaSettings = allSla
  }
  const query =
    'mutation' +
    '($userCredentialKey: String, $userCredentialPassword: String, $parentCredentialKey: String, $parentCredentialPassword: String, $clientDetailMeterNumber: String, $clientDetailAccountNumber: String, $isLive: Boolean, $residential: Boolean,$optimizeShippingType: Int,$unitWeight: String,$unitDimension: String,$packingAccessKey: String,$slaSettings:[SlaSettingsInput])' +
    '{saveAppSetting(appSetting: {userCredentialKey:$userCredentialKey,userCredentialPassword:$userCredentialPassword,parentCredentialKey:$parentCredentialKey,parentCredentialPassword:$parentCredentialPassword,clientDetailMeterNumber:$clientDetailMeterNumber,clientDetailAccountNumber:$clientDetailAccountNumber,isLive:$isLive,residential:$residential,optimizeShippingType:$optimizeShippingType,unitWeight:$unitWeight,unitDimension:$unitDimension,packingAccessKey:$packingAccessKey,slaSettings:$slaSettings})' +
    '@context(provider: "vtexus.fedex-shipping")}'

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
