export default {
  loadDocksAPI: (baseUrl) => {
    return `${baseUrl}/admin/shipping-strategy/loading-docks/`
  },
  calculateShippingAPI: (account, workspace) => {
    return `https://app.io.vtex.com/vtexus.fedex-shipping/v1/${account}/${workspace}/shp-rates/calculate`
  },
}
