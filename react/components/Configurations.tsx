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
import { fedexHandling } from '../utils/constants'

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
    residential: boolean
    optimizeShipping: boolean
    unitWeight: string
    unitDimension: string
    itemModals: any[]
    slaSettings: any[]
  }>({
    clientDetailMeterNumber: '',
    clientDetailAccountNumber: '',
    userCredentialKey: '',
    userCredentialPassword: '',
    isLive: false,
    residential: false,
    optimizeShipping: false,
    unitWeight: 'LB',
    unitDimension: 'IN',
    itemModals: [],
    slaSettings: [],
  })

  const {
    clientDetailMeterNumber,
    clientDetailAccountNumber,
    userCredentialKey,
    userCredentialPassword,
    isLive,
    residential,
    optimizeShipping,
    unitWeight,
    unitDimension,
    itemModals: items,
    slaSettings,
  } = state

  const [saveAppSetting] = useMutation(SaveAppSetting)

  useEffect(() => {
    if (!data?.getAppSettings) return

    const { getAppSettings } = data

    getAppSettings.itemModals.forEach((itemModal: any, index: number) => {
      itemModal.id = index
    })

    getAppSettings.slaSettings.forEach((slaSetting: any, index: number) => {
      slaSetting.id = index
    })

    setState({ ...getAppSettings })
  }, [data])

  // Prefills array of modal list size
  const dropdownStates: any[] = Array(7).fill(0)

  const checkboxModalStates: any[] = Array(10).fill(false)

  const checkboxSlaStates: any[] = Array(9).fill(false)

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

  const setSurcharge = (value: number, surchargeType: string, id: number) => {
    const newSlaSettings = slaSettings

    newSlaSettings[id][surchargeType] = value
    setState({ ...state, slaSettings: newSlaSettings })
  }

  const slaGridState = useDataGridState({
    columns: [
      {
        id: 'sla',
        header: 'SLA Name',
        accessor: 'sla',
      },
      {
        id: 'hidden',
        header: 'Hide SLA',
        accessor: 'hidden',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            checkboxSlaStates[item.id] = UseCheckboxState({
              state: item.hidden,
            })

            return (
              <Checkbox
                csx={{ margin: 'auto' }}
                state={checkboxSlaStates[item.id]}
              />
            )
          },
        },
      },
      {
        id: 'surchargeFlat',
        header: 'Surcharge Flat Rate',
        accessor: 'surchargeFlatRate',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            return (
              <Set>
                <Input
                  id={item.id}
                  label="Flat Rate Surcharge"
                  charLimit={7}
                  value={item.surchargeFlatRate}
                  onChange={(e) => {
                    let inputVal = e.target.value

                    if (inputVal.endsWith('.')) {
                      inputVal += '00'
                    }

                    const regexp = /^\d{0,4}(?:[.,]\d{1,2})?$/

                    if (regexp.test(inputVal)) {
                      setSurcharge(
                        parseFloat(inputVal),
                        'surchargeFlatRate',
                        item.id
                      )
                    }
                  }}
                />
              </Set>
            )
          },
        },
      },
      {
        id: 'surchargePct',
        header: 'Surcharge Percentage',
        accessor: 'surchargePercent',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            return (
              <Set>
                <Input
                  id={item.id}
                  label="Percent Surcharge"
                  suffix="%"
                  charLimit={3}
                  value={item.surchargePercent}
                  onChange={(e) => {
                    const inputVal =
                      e.target.value === '-' || e.target.value.length === 0
                        ? '0'
                        : e.target.value

                    if (!Number.isNaN(inputVal)) {
                      setSurcharge(
                        parseInt(inputVal, 10),
                        'surchargePercent',
                        item.id
                      )
                    }
                  }}
                />
              </Set>
            )
          },
        },
      },
    ],
    items: slaSettings,
  })

  const showToast = useToast()

  const handleSave = () => {
    const saveModals: any[] = []

    const saveSlaSettings: SlaSetting[] = []

    dropdownStates.forEach((dropdown, index) => {
      saveModals.push({
        modal: items[index].modal,
        fedexHandling: dropdown.selectedItem,
        shipAlone: checkboxModalStates[index].state,
      })
    })

    slaSettings.forEach((sla, index) => {
      saveSlaSettings.push({
        sla: sla.sla,
        hidden: checkboxSlaStates[index].state,
        surchargePercent: sla.surchargePercent,
        surchargeFlatRate: sla.surchargeFlatRate,
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
          residential,
          optimizeShipping,
          unitWeight,
          unitDimension,
          itemModals: saveModals,
          slaSettings: saveSlaSettings,
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

  const generateItemModalMapping = () => {
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

  const generateSLAMapping = () => {
    const slaMap = UseCollapsibleState()

    return (
      <Collapsible state={slaMap} disabled={false}>
        <CollapsibleHeader
          label={formatMessage({ id: 'admin/fedex-shipping.modifySLA' })}
        />
        <CollapsibleContent>
          <DataGrid state={slaGridState} />
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
            checked={residential}
            onChange={() => setState({ ...state, residential: !residential })}
          />
          <Label>
            {formatMessage({ id: 'admin/fedex-shipping.residential' })}
          </Label>
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
          <option value="G">
            {formatMessage({ id: 'admin/fedex-shipping.g' })}
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
        <Heading className="pt3">
          {formatMessage({ id: 'admin/fedex-shipping.modifySLA' })}
        </Heading>
        <Text variant="body">
          {formatMessage({ id: 'admin/fedex-shipping.modifySLA.description' })}
        </Text>
        {generateSLAMapping()}
      </Set>
      <Set orientation="vertical" spacing={1}>
        <Heading className="pt3">
          {formatMessage({ id: 'admin/fedex-shipping.modalMap' })}
        </Heading>
        <Text variant="body">
          {formatMessage({ id: 'admin/fedex-shipping.modalMap.description' })}
        </Text>
        {generateItemModalMapping()}
      </Set>
      <Button variant="primary" onClick={() => handleSave()}>
        {formatMessage({ id: 'admin/fedex-shipping.saveSettings' })}
      </Button>
    </PageContent>
  )
}

export default Configurations
