import { testSetup, preserveCookie } from '../support/common/support.js'
import { loadDocks, calculateShipping } from '../support/fedex.apis'
import { data } from '../support/fedex.outputvalidation'

describe('Rest-api-testcases', () => {
  testSetup()

  loadDocks()

  calculateShipping(data)

  preserveCookie()
})
