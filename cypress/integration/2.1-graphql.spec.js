import {
  preserveCookie,
  updateRetry,
  testSetup,
} from '../support/common/support'
import {
  graphql,
  getAppSettings,
  getDocks,
  saveAppSetting,
  updateDockConnection,
  validateGetAppSettingsResponse,
  validateSaveAppSettingResponse,
  validateUpdateDockConnectionResponse,
  validateGetDockConnectionResponse,
} from '../support/fedex.graphql'
import { appSetting, slaName, dockId } from '../support/fedex.outputvalidation'

const prefix = 'Graphql testcase'

describe('FedEx GraphQL Validation', () => {
  testSetup()

  it(`${prefix} - Get App Settings`, updateRetry(3), () => {
    graphql(getAppSettings(), validateGetAppSettingsResponse)
  })

  it(`${prefix} - save App Settings`, updateRetry(3), () => {
    graphql(
      saveAppSetting(appSetting, slaName, false),
      validateSaveAppSettingResponse
    )
  })

  it(`${prefix} - Update Dock Connection`, updateRetry(3), () => {
    graphql(
      updateDockConnection(dockId, false),
      validateUpdateDockConnectionResponse
    )
  })

  it(`${prefix} - Get Docks`, updateRetry(3), () => {
    graphql(getDocks(), validateGetDockConnectionResponse)
  })

  preserveCookie()
})
