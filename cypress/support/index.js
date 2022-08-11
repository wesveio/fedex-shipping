import './common/commands.js'
import './common/api_commands.js'
import './common/env_orders.js'
import './commands'

// Configure it to preserve cookies
Cypress.Cookies.defaults({
  preserve: /VtexIdclientAutCookie/,
})
