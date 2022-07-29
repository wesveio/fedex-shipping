import { FAIL_ON_STATUS_CODE } from './common/constants'

const config = Cypress.env()

// Constants
const { vtex } = config.base

export function graphql(
  appName,
  getQuery,
  validateResponseFn = null,
  params = null
) {
  const { query, queryVariables } = getQuery

  // Define constants
  const APP_VERSION = '*.x'
  const APP_NAME = appName
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
      validateResponseFn(response, params)
    })
  } else {
    return cy.get('@RESPONSE')
  }
}

export function getAppSettings() {
  return {
    query:
      'query' +
      '{ getAppSettings{userCredentialKey,userCredentialPassword,parentCredentialKey,parentCredentialPassword,clientDetailAccountNumber,clientDetailMeterNumber,isLive,residential,optimizeShippingType,unitWeight,unitDimension,packingAccessKey}}',
  }
}

export function getDocks() {
  return {
    query: 'query' + '{getDocks{docksList{id,name,shippingRatesProviders}}}',
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

export function verifyInventoryIsUnlimitedForFedexWareHouse(warehouseId, sku) {
  const query =
    'query' +
    '($sku: ID!, $warehouseId: ID!)' +
    '{inventoryProduct(sku:$sku,warehouseId:$warehouseId){unlimited}}'

  return {
    query,
    queryVariables: { sku, warehouseId },
  }
}

export function validateInventory(response) {
  expect(response.body.data.inventoryProduct.unlimited).to.equal(true)
}

export function loadingDock(id) {
  const query = 'query' + '($id: ID!)' + '{loadingDock(id:$id){isActive}}'

  return {
    query,
    queryVariables: { id },
  }
}

export function verifyDockisActive(response) {
  expect(response.body.data.loadingDock.isActive).to.equal(true)
}

export function warehouse(id) {
  const query =
    'query' +
    '($id: ID!)' +
    '{warehouse(id:$id){isActive,warehouseDocks{dockId}}}'

  return {
    query,
    queryVariables: { id },
  }
}

export function validateWareHouseIsActiveAndLinkedWithDocks(
  response,
  dockValues
) {
  const { isActive, warehouseDocks } = response.body.data.warehouse

  expect(isActive).to.equal(true)
  const [actualDockId1, actualDockId2] = [
    warehouseDocks[0].dockId,
    warehouseDocks[1].dockId,
  ]

  const [expectedDockId1, expectedDockId2] = [
    dockValues[0].id,
    dockValues[1].id,
  ]

  expect(actualDockId1).to.equal(expectedDockId1)
  expect(actualDockId2).to.equal(expectedDockId2)
}

export function validateGetAppSettingsResponse(response) {
  expect(response.body.data.getAppSettings).to.not.equal(null)
}

export function validateGetDockConnectionResponse(response) {
  const { docksList } = response.body.data.getDocks

  expect(docksList).to.be.an('array').and.to.have.lengthOf.above(0)
  const results = docksList.filter(
    ({ shippingRatesProviders, name }) =>
      shippingRatesProviders.length > 1 && name.includes('Fedex')
  )

  expect(results.length).to.equal(2)
}

export function validateSaveAppSettingResponse(response) {
  expect(response.body.data.saveAppSetting).to.equal(true)
}

export function validateUpdateDockConnectionResponse(response) {
  expect(response.body.data.updateDockConnection).to.equal(true)
}
