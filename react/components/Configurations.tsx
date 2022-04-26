/* eslint-disable @typescript-eslint/no-explicit-any */
import type { FC } from 'react'
import React, { useState, useEffect } from 'react'
import { useIntl } from 'react-intl'
import {
  PageContent,
  Input,
  InputPassword,
  Heading,
  Toggle,
  Button,
  useToast,
  Set,
  Select,
  CheckboxGroup,
  Label,
  Checkbox,
  Text,
  Collapsible,
  CollapsibleHeader,
  CollapsibleContent,
  useDataGridState,
  DataGrid,
  IconWarning,
  IconCheckCircle,
  Center,
  Dropdown,
  Tooltip,
  useCheckboxState as UseCheckboxState,
  useCollapsibleState as UseCollapsibleState,
  useDropdownState as UseDropdownState,
} from '@vtex/admin-ui'
import { useQuery, useMutation } from 'react-apollo'

import AppSettings from '../queries/getAppSettings.gql'
import SaveAppSetting from '../mutations/saveAppSetting.gql'
import { fedexHandling, slaList } from '../utils/constants'

const Configurations: FC = () => {
  const { formatMessage } = useIntl()

  const { data } = useQuery(AppSettings, {
    ssr: false,
  })

  const [state, setState] = useState<{
    clientDetailMeterNumber: string
    clientDetailAccountNumber: string
    userCredentialKey: string
    userCredentialPassword: string
    isLive: boolean
    optimizeShipping: boolean
    unitWeight: string
    unitDimension: string
    hiddenSLA: string[]
    itemModals: any[]
  }>({
    clientDetailMeterNumber: '',
    clientDetailAccountNumber: '',
    userCredentialKey: '',
    userCredentialPassword: '',
    isLive: false,
    optimizeShipping: false,
    unitWeight: 'LB',
    unitDimension: 'IN',
    hiddenSLA: [],
    itemModals: [],
  })

  const {
    clientDetailMeterNumber,
    clientDetailAccountNumber,
    userCredentialKey,
    userCredentialPassword,
    isLive,
    optimizeShipping,
    unitWeight,
    unitDimension,
    hiddenSLA,
    itemModals: items,
  } = state

  const [saveAppSetting] = useMutation(SaveAppSetting)

  const checkbox = UseCheckboxState({ state: hiddenSLA })

  useEffect(() => {
    if (!data?.getAppSettings) return

    const { getAppSettings } = data

    checkbox.setState(getAppSettings.hiddenSLA)

    getAppSettings.itemModals.forEach((itemModal: any, index: number) => {
      itemModal.id = index
    })

    setState({ ...getAppSettings })
  }, [data])

  // Prefills array of modal list size
  const dropdownStates: any[] = Array(7).fill(0)

  const checkboxModalStates: any[] = Array(10).fill(false)

  const modalGridState = useDataGridState({
    columns: [
      {
        id: 'modal',
        header: 'Modal Name',
        accessor: 'modal',
      },
      {
        id: 'shipAlone',
        header: 'Ship Alone',
        accessor: 'shipAlone',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            checkboxModalStates[item.id] = UseCheckboxState({
              state: item.shipAlone,
            })

            return (
              <Checkbox
                csx={{ margin: 'auto' }}
                state={checkboxModalStates[item.id]}
              />
            )
          },
        },
      },
      {
        id: 'fedexHandling',
        header: 'FedEx Handling Method',
        accessor: 'fedexHandling',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            dropdownStates[item.id] = UseDropdownState({
              items: fedexHandling,
              initialSelectedItem: item.fedexHandling,
            })

            const isDangerousGoods = !dropdownStates[item.id]?.selectedItem
              ? item.fedexHandling === 'NONE'
              : dropdownStates[item.id]?.selectedItem === 'NONE'

            return (
              <Set>
                <Dropdown
                  variant="secondary"
                  csx={{ width: 300 }}
                  items={fedexHandling}
                  state={dropdownStates[item.id]}
                  label="fedexHandling"
                />
                <Center>
                  {isDangerousGoods ? (
                    <Tooltip label="No Special Handling" placement="right">
                      <IconCheckCircle />
                    </Tooltip>
                  ) : (
                    <Tooltip
                      label="Dangerous Goods. Requires FedEx Special Handling"
                      placement="right"
                    >
                      <IconWarning />
                    </Tooltip>
                  )}
                </Center>
              </Set>
            )
          },
        },
      },
    ],
    items,
  })

  const showToast = useToast()

  const handleSave = () => {
    const saveModals: any[] = []

    dropdownStates.forEach((dropdown, index) => {
      saveModals.push({
        modal: items[index].modal,
        fedexHandling: dropdown.selectedItem,
        shipAlone: checkboxModalStates[index].state,
      })
    })
    saveAppSetting({
      variables: {
        appSetting: {
          userCredentialKey,
          userCredentialPassword,
          parentCredentialKey: '',
          parentCredentialPassword: '',
          clientDetailMeterNumber,
          clientDetailAccountNumber,
          isLive,
          optimizeShipping,
          unitWeight,
          unitDimension,
          hiddenSLA: checkbox.state,
          itemModals: saveModals,
        },
      },
    }).then((result: any) => {
      let message = 'Settings Saved'

      if (!result?.data?.saveAppSetting) {
        message = 'Error. Fail to Save'
      }

      showToast({
        message,
      })
    })
  }

  const generateCheckboxGroup = () => {
    const checkboxes = slaList.map((sla: string) => {
      return (
        <Label key={sla}>
          <Checkbox state={checkbox} value={sla} />
          {sla}
        </Label>
      )
    })

    return <CheckboxGroup orientation="vertical">{checkboxes}</CheckboxGroup>
  }

  const generateModalMapping = () => {
    const modalMap = UseCollapsibleState()

    return (
      <Collapsible state={modalMap} disabled={false}>
        <CollapsibleHeader
          label={formatMessage({ id: 'admin/fedex-shipping.modalMap' })}
        />
        <CollapsibleContent>
          <DataGrid state={modalGridState} />
        </CollapsibleContent>
      </Collapsible>
    )
  }

  return (
    <PageContent>
      <Set orientation="vertical" spacing={2}>
        <Heading className="pt6">
          {formatMessage({ id: 'admin/fedex-shipping.settings' })}
        </Heading>
        <Input
          csx={{ width: 250 }}
          id="meter"
          label={formatMessage({ id: 'admin/fedex-shipping.meter' })}
          value={clientDetailMeterNumber}
          onChange={(e) =>
            setState({ ...state, clientDetailMeterNumber: e.target.value })
          }
        />
        <Input
          csx={{ width: 250 }}
          id="accountNumber"
          label={formatMessage({ id: 'admin/fedex-shipping.accountNum' })}
          value={clientDetailAccountNumber}
          onChange={(e) =>
            setState({ ...state, clientDetailAccountNumber: e.target.value })
          }
        />
        <InputPassword
          csx={{ width: 250 }}
          id="credentialKey"
          label={formatMessage({ id: 'admin/fedex-shipping.credKey' })}
          value={userCredentialKey}
          onChange={(e) =>
            setState({ ...state, userCredentialKey: e.target.value })
          }
        />
        <InputPassword
          csx={{ width: 250 }}
          id="credentialPwd"
          label={formatMessage({ id: 'admin/fedex-shipping.credPwd' })}
          value={userCredentialPassword}
          onChange={(e) =>
            setState({ ...state, userCredentialPassword: e.target.value })
          }
        />
        <Set orientation="horizontal" spacing={2}>
          <Toggle
            aria-label="label"
            checked={isLive}
            onChange={() => setState({ ...state, isLive: !isLive })}
          />
          <Label>{formatMessage({ id: 'admin/fedex-shipping.isLive' })}</Label>
        </Set>
        <Set orientation="horizontal" spacing={2}>
          <Toggle
            aria-label="label"
            checked={optimizeShipping}
            onChange={() =>
              setState({ ...state, optimizeShipping: !optimizeShipping })
            }
          />
          <Label>
            {formatMessage({ id: 'admin/fedex-shipping.optimizeShipping' })}
          </Label>
        </Set>
      </Set>
      <Set orientation="vertical" spacing={2}>
        <Heading className="pt6">
          {formatMessage({ id: 'admin/fedex-shipping.unitsMeasurement' })}
        </Heading>
        <Select
          label={formatMessage({ id: 'admin/fedex-shipping.weight' })}
          value={unitWeight ?? 'LB'}
          onChange={(e) => setState({ ...state, unitWeight: e.target.value })}
        >
          <option value="LB">
            {formatMessage({ id: 'admin/fedex-shipping.lb' })}
          </option>
          <option value="KG">
            {formatMessage({ id: 'admin/fedex-shipping.kg' })}
          </option>
        </Select>
        <Select
          label={formatMessage({ id: 'admin/fedex-shipping.dimensions' })}
          value={unitDimension ?? 'IN'}
          onChange={(e) =>
            setState({ ...state, unitDimension: e.target.value })
          }
        >
          <option value="IN">
            {formatMessage({ id: 'admin/fedex-shipping.in' })}
          </option>
          <option value="CM">
            {formatMessage({ id: 'admin/fedex-shipping.cm' })}
          </option>
        </Select>
      </Set>
      <Set orientation="vertical" spacing={1}>
        <Heading className="pt6">
          {formatMessage({ id: 'admin/fedex-shipping.hiddenSLA' })}
        </Heading>
        {generateCheckboxGroup()}
      </Set>
      <Set orientation="vertical" spacing={1}>
        <Heading className="pt3">
          {formatMessage({ id: 'admin/fedex-shipping.modalMap' })}
        </Heading>
        <Text variant="body">
          {formatMessage({ id: 'admin/fedex-shipping.modalMap.description' })}
        </Text>
        {generateModalMapping()}
      </Set>
      <Button variant="primary" onClick={() => handleSave()}>
        {formatMessage({ id: 'admin/fedex-shipping.saveSettings' })}
      </Button>
    </PageContent>
  )
}

export default Configurations
