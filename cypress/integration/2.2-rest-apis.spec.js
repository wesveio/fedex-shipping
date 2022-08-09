import { loginViaCookies, preserveCookie } from '../support/common/support.js'
import {
  loadDocks,
  loadCalculateShippingAPI,
  validateCalculateShipping,
} from '../support/api_testcase.js'
import { data } from '../fixtures/shippingRatePayload.json'

describe('Rest-api-testcases', () => {
  loginViaCookies()

  loadDocks()

  loadCalculateShippingAPI(data, validateCalculateShipping)

  preserveCookie()
})
