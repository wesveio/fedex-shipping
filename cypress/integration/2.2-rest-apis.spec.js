import { testSetup, preserveCookie } from '../support/common/support.js'
import { loadDocks, calculateShipping } from '../support/api_testcase.js'
import { data } from '../fixtures/shippingRatePayload.json'

describe('Rest-api-testcases', () => {
  testSetup()

  loadDocks()

  calculateShipping(data)

  preserveCookie()
})
