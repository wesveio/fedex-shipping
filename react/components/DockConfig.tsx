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
  Toggle,
  Set,
  useToast,
  useDataGridState,
  usePaginationState,
  useDataViewState,
} from '@vtex/admin-ui'
import { useMutation, useQuery } from 'react-apollo'

import GetDocks from '../queries/getDocks.gql'
import UpdateDockConnection from '../mutations/updateDockConnection.gql'

const DockConfig: FC = () => {
  const { data } = useQuery(GetDocks, {
    ssr: false,
  })

  const showToast = useToast()

  const [updateDockConnection] = useMutation(UpdateDockConnection)

  const view = useDataViewState()

  const [state, setState] = useState<{
    docksList: any[]
    dockEnabled: {
      [name: string]: boolean
    }
  }>({
    docksList: [],
    dockEnabled: {},
  })

  const { docksList, dockEnabled } = state

  useEffect(() => {
    if (!data?.getDocks) return

    const { getDocks } = data

    const dockEnabledMap: any = {}

    getDocks.docksList.forEach((dock: any) => {
      dockEnabledMap[dock.id] = dock.shippingRatesProviders.includes(
        'vtexus.fedex-shipping'
      )
    })

    setState({ ...getDocks, dockEnabled: dockEnabledMap })
  }, [data])

  const toggleDock = (dockId: string) => {
    dockEnabled[dockId] = !dockEnabled[dockId]

    updateDockConnection({
      variables: {
        updateDock: {
          dockId,
          toRemove: !dockEnabled[dockId],
        },
      },
    }).then((result: any) => {
      setState({ ...state, dockEnabled })

      const message = result?.data?.updateDockConnection
        ? 'Dock Updated'
        : 'Please Try Again'

      showToast({
        message,
      })
    })
  }

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
              <Set>
                <Center>
                  {dockEnabled[item.id] ? <IconCheckCircle /> : <IconWarning />}
                </Center>
              </Set>
            )
          },
        },
      },
      {
        id: 'changeConnection',
        header: 'Change Status',
        accessor: 'shippingRatesProviders',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            return (
              <Set>
                <Center>
                  <Toggle
                    aria-label="label"
                    checked={dockEnabled[item.id]}
                    onChange={() => toggleDock(item.id)}
                  />
                </Center>
              </Set>
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
