import type { FC } from 'react'
import React, { useEffect, useState } from 'react'
import {
  PageContent,
  DataGrid,
  IconWarning,
  IconCheckCircle,
  Center,
  Pagination,
  DataViewControls,
  DataView,
  useDataGridState,
  usePaginationState,
  useDataViewState,
} from '@vtex/admin-ui'
import { useQuery } from 'react-apollo'

import GetDocks from '../queries/getDocks.gql'

const DockConfig: FC = () => {
  const { data } = useQuery(GetDocks, {
    ssr: false,
  })

  const view = useDataViewState()

  const [state, setState] = useState<{
    docksList: any[]
  }>({
    docksList: [],
  })

  const { docksList } = state

  useEffect(() => {
    if (!data?.getDocks) return

    const { getDocks } = data

    setState({ ...getDocks })
  }, [data])

  const pagination = usePaginationState({
    pageSize: 10,
    total: 200,
  })

  const tableState = useDataGridState({
    columns: [
      {
        id: 'id',
        header: 'Id',
        accessor: 'id',
      },
      {
        id: 'name',
        header: 'Name',
        accessor: 'name',
      },
      {
        id: 'connected',
        header: 'Connected',
        accessor: 'shippingRatesProviders',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            return (
              <Center>
                {item.shippingRatesProviders.includes(
                  'vtexus.fedex-shipping'
                ) ? (
                  <IconCheckCircle />
                ) : (
                  <IconWarning />
                )}
              </Center>
            )
          },
        },
      },
    ],
    items: docksList.slice(pagination.range[0] - 1, pagination.range[1]),
    length: docksList.length,
  })

  return (
    <PageContent>
      <DataView state={view}>
        <DataViewControls>
          <Pagination
            state={pagination}
            preposition="of"
            subject="results"
            prevLabel="Previous"
            nextLabel="Next"
          />
        </DataViewControls>
        <DataGrid state={tableState} />
      </DataView>
    </PageContent>
  )
}

export default DockConfig
