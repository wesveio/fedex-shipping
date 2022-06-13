import type { FC } from 'react'
import React from 'react'
import { useIntl } from 'react-intl'
import {
  Heading,
  Set,
  Select,
  Text,
  Collapsible,
  CollapsibleHeader,
  CollapsibleContent,
  DataGrid,
  useCollapsibleState as UseCollapsibleState,
  useCheckboxState as UseCheckboxState,
  useDropdownState as UseDropdownState,
  Center,
  Checkbox,
  Dropdown,
  IconCheckCircle,
  IconWarning,
  Input,
  Tooltip,
  Button,
  useDataGridState,
} from '@vtex/admin-ui'
import PropTypes from 'prop-types'

import { fedexHandling } from '../utils/constants'

const AdvanceConfigurations: FC<any> = (props: any) => {
  const { formatMessage } = useIntl()

  const checkboxSlaStates: any[] = Array(9).fill(false)

  const checkboxModalStates: any[] = Array(10).fill(false)

  const dropdownStates: any[] = Array(7).fill(0)

  const setSurcharge = (value: number, surchargeType: string, id: number) => {
    const newSlaSettings = props.slaSettings

    newSlaSettings[id][surchargeType] = value

    props.setChange('slaSettings', newSlaSettings)
  }

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
          type: 'root',
          render: ({ item }: any) => {
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
          type: 'root',
          render: ({ item }: any) => {
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
    items: props.items,
  })

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
          type: 'root',
          render: ({ item }: any) => {
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
          type: 'root',
          render: ({ item }: any) => {
            return (
              <Set>
                <Input
                  id={`${item.id}-flat`}
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
          type: 'root',
          render: ({ item }: any) => {
            return (
              <Set>
                <Input
                  id={`${item.id}-pct`}
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
    items: props.slaSettings,
  })

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

  const saveSettings = () => {
    const saveModals: any[] = []

    const saveSlaSettings: SlaSetting[] = []

    dropdownStates.forEach((dropdown, index) => {
      saveModals.push({
        modal: props.items[index].modal,
        fedexHandling: dropdown.selectedItem,
        shipAlone: checkboxModalStates[index].state,
      })
    })

    props.slaSettings.forEach((sla: any, index: number) => {
      saveSlaSettings.push({
        sla: sla.sla,
        hidden: checkboxSlaStates[index].state,
        surchargePercent: sla.surchargePercent,
        surchargeFlatRate: sla.surchargeFlatRate,
      })
    })

    props.handleAdvanceSave(saveModals, saveSlaSettings)
  }

  return (
    <Set orientation="vertical" spacing={2}>
      <Set orientation="vertical" spacing={2}>
        <Heading className="pt6">
          {formatMessage({ id: 'admin/fedex-shipping.unitsMeasurement' })}
        </Heading>
        <Select
          label={formatMessage({ id: 'admin/fedex-shipping.weight' })}
          value={props.unitWeight ?? 'LB'}
          onChange={(e) => props.setChange('unitWeight', e.target.value)}
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
          value={props.unitDimension ?? 'IN'}
          onChange={(e) => props.setChange('unitDimension', e.target.value)}
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
          {formatMessage({
            id: 'admin/fedex-shipping.modifySLA.description',
          })}
        </Text>
        {generateSLAMapping()}
      </Set>
      <Set orientation="vertical" spacing={1}>
        <Heading className="pt3">
          {formatMessage({ id: 'admin/fedex-shipping.modalMap' })}
        </Heading>
        <Text variant="body">
          {formatMessage({
            id: 'admin/fedex-shipping.modalMap.description',
          })}
        </Text>
        {generateItemModalMapping()}
      </Set>
      <Button variant="primary" onClick={() => saveSettings()}>
        {formatMessage({ id: 'admin/fedex-shipping.saveSettings' })}
      </Button>
    </Set>
  )
}

AdvanceConfigurations.defaultProps = {
  handleAdvanceSave: () => undefined,
  setChange: () => undefined,
  unitDimension: '',
  unitWeight: '',
  slaSettings: [],
  items: [],
}

AdvanceConfigurations.propTypes = {
  handleAdvanceSave: PropTypes.func,
  setChange: PropTypes.func,
  unitDimension: PropTypes.string,
  unitWeight: PropTypes.string,
  slaSettings: PropTypes.array,
  items: PropTypes.array,
}

export default AdvanceConfigurations
