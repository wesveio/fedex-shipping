import { testSetup, preserveCookie } from '../support/common/support.js'
import { loadDocks } from '../support/fedex.apis'

describe('Rest-api-testcases', () => {
  testSetup()

  loadDocks()

  preserveCookie()
})
