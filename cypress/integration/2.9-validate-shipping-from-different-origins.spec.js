import { loginViaCookies, updateRetry } from '../support/common/support.js'
import { data } from '../fixtures/shippingRatePayload.json'
import {
  loadCalculateShippingAPI,
  validateInternationEconomyShipping,
} from '../support/api_testcase.js'
import sla from '../support/sla.js'

describe('Validate Shipping from different origins', () => {
  loginViaCookies()

  it(
    'Shipment USA to Italy ( Origin -> destination = USA -> ITA)',
    updateRetry(3),
    () => {
      data.destination = {
        zipCode: '06010',
        country: 'ITA',
        state: null,
        city: null,
        coordinates: null,
        residential: false,
      }
      cy.addDelayBetweenRetries(3000)
      loadCalculateShippingAPI(data).then((response) => {
        expect(response.status).to.have.equal(200)
        expect(response.body).to.be.an('array').and.to.have.lengthOf.above(0)
        const filtershippingMethod = response.body.filter(
          (b) => b.shippingMethod === sla.InternationalEconomy
        )

        expect(filtershippingMethod)
          .to.be.an('array')
          .and.to.have.lengthOf.above(0)
      })
    }
  )

  it(
    'Shipment Italy to USA ( Origin -> destination = ITA -> USA)',
    updateRetry(3),
    () => {
      data.destination = {
        zipCode: '33301',
        country: 'USA',
        state: null,
        city: null,
        coordinates: null,
        residential: false,
      }
      data.origin = {
        zipCode: '06010',
        country: 'ITA',
        state: null,
        city: null,
        coordinates: null,
        residential: false,
      }
      cy.addDelayBetweenRetries(3000)
      loadCalculateShippingAPI(data, validateInternationEconomyShipping)
    }
  )

  it(
    'Use Poland shipment and verify error message displays ',
    updateRetry(3),
    () => {
      data.destination = {
        zipCode: '00-005',
        country: 'PL',
        state: null,
        city: null,
        coordinates: null,
        residential: false,
      }
      cy.addDelayBetweenRetries(3000)
      loadCalculateShippingAPI(data).then((response) => {
        expect(response.status).to.have.equal(500)
      })
    }
  )
})
