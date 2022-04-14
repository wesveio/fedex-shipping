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
  useCheckboxState,
  CheckboxGroup,
  Label,
  Checkbox,
  Text,
  useCollapsibleState,
  Collapsible,
  CollapsibleHeader,
  CollapsibleContent,
  useDataGridState,
  DataGrid,
  IconXCircle,
  IconCheckCircle,
  Center,
  Dropdown,
  useDropdownState
} from '@vtex/admin-ui'
import { useQuery, useMutation } from 'react-apollo'
import AppSettings from '../queries/getAppSettings.gql'
import SaveAppSetting from '../mutations/saveAppSetting.gql'

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
    hiddenSLA: any[]
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

  const [slaState] = useState<any>({
    slaList: ['FedEx Ground', 'Priority Overnight', 'Express Saver', '2DAY AM', 'First Overnight', 'Standard Overnight', '2Day', 'FedEx Home Delivery']
  })

  const { clientDetailMeterNumber, clientDetailAccountNumber, userCredentialKey, userCredentialPassword, isLive, optimizeShipping, unitWeight, unitDimension, hiddenSLA, itemModals: items } = state
  const { slaList } = slaState

  const [saveAppSetting] = useMutation(SaveAppSetting);

  let checkbox = useCheckboxState({ state: hiddenSLA })

  useEffect(() => {
    if (!data?.getAppSettings) return

    let getAppSettings = data.getAppSettings
    checkbox.setState(getAppSettings.hiddenSLA)

    getAppSettings.itemModals.forEach((itemModal: any, index: number) => {
      itemModal.id = index
    })

    setState({...getAppSettings})
  }, [data])

  // Prefills array of modal list size
  let dropdownStates: any[] = Array(7).fill(0)

  let modalGridState = useDataGridState({
    columns: [
      {
        id: 'modal',
        header: 'Modal Name',
        accessor: 'modal',
      },
      {
        id: 'isDangerous',
        header: 'Dangerous Goods',
        accessor: 'fedexHandling',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            let isDangerousGoods = !dropdownStates[item.id]?.selectedItem ? item.fedexHandling === 'NONE' : dropdownStates[item.id]?.selectedItem === 'NONE'
            return (
              <Center>
                {isDangerousGoods ? <IconXCircle/> : <IconCheckCircle />}
              </Center>
            )
          }
        }
      },
      {
        id: 'fedexHandling',
        header: 'FedEx Handling Method',
        accessor: 'fedexHandling',
        resolver: {
          type: 'plain',
          render: ({ item }) => {
            const fedexHandling = ['BATTERY', 'HAZARDOUS_MATERIALS', 'LIMITED_QUANTITIES_COMMODITIES', 'ORM_D', 'REPORTABLE_QUANTITIES', 'SMALL_QUANTITY_EXCEPTION', 'NONE']
            dropdownStates[item.id] = useDropdownState({
              items: fedexHandling,
              initialSelectedItem: item.fedexHandling,
            })
            
            return (
              <div>
                <Dropdown
                  items={fedexHandling}
                  state={dropdownStates[item.id]}
                  label='fedexHandling'
                />
              </div>
            )
          }
        }
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
        fedexHandling: dropdown.selectedItem
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
          itemModals: saveModals
        }
      }
    }).then((result: any) => {
      let message = 'Settings Saved'
      if (!result?.data?.saveAppSetting) {
        message = 'Error. Fail to Save'
      }
      showToast({
        message
      })
    })
  }

  const generateCheckboxGroup = () => {
    const checkboxes = slaList.map((sla: any) => {
      return (
        <Label key={sla}>
          <Checkbox state={checkbox} value={sla} />
          {sla}
        </Label>
      )
    })

    return (
      <CheckboxGroup orientation='vertical'>
        {checkboxes}
      </CheckboxGroup>
    )
  }

  const generateModalMapping = () => {
    const modalMap = useCollapsibleState()

    return (
      <Collapsible state={modalMap} disabled={false}>
        <CollapsibleHeader label={formatMessage({id: 'admin/fedex-shipping.modalMap'})}/>
        <CollapsibleContent>
          <DataGrid state={modalGridState} />
        </CollapsibleContent>
      </Collapsible>
    )
  }


  return (
    <PageContent>
      <div className='pv6'>
        <Heading>{formatMessage({id: 'admin/fedex-shipping.settings'})}</Heading>
        <Set orientation='vertical' spacing={3}>
          <Input id='meter' label={formatMessage({id: 'admin/fedex-shipping.meter'})} value={clientDetailMeterNumber} 
            onChange={(e) => setState({...state, clientDetailMeterNumber: e.target.value})}
            />
          <Input id='accountNumber' label={formatMessage({id: 'admin/fedex-shipping.accountNum'})} value={clientDetailAccountNumber}
            onChange={(e) => setState({...state, clientDetailAccountNumber: e.target.value})}
            />
          <InputPassword id='credentialKey' label={formatMessage({id: 'admin/fedex-shipping.credKey'})} value={userCredentialKey}
            onChange={(e) => setState({...state, userCredentialKey: e.target.value})}
            />
          <InputPassword id='credentialPwd' label={formatMessage({id: 'admin/fedex-shipping.credPwd'})} value={userCredentialPassword}
            onChange={(e) => setState({...state, userCredentialPassword: e.target.value})}
          />
          <Label>
            <Toggle
              aria-label='label'
              checked={isLive}
              onChange={() => setState({...state, isLive: !isLive})}
            />
            {formatMessage({id: 'admin/fedex-shipping.isLive'})}
          </Label>
          <Label>
            <Toggle
              aria-label='label'
              checked={optimizeShipping}
              onChange={() => setState({...state, optimizeShipping: !optimizeShipping})}
            />
            {formatMessage({id: 'admin/fedex-shipping.optimizeShipping'})}
          </Label>
        </Set>
        <Heading>{formatMessage({id: 'admin/fedex-shipping.unitsMeasurement'})}</Heading>
        <Set className='pt6' spacing={3}>
          <Select
            label={formatMessage({id: 'admin/fedex-shipping.weight'})}
            value={unitWeight ?? 'LB'}
            onChange={(e) => setState({...state, unitWeight: e.target.value})}
          >
            <option value='LB'>{formatMessage({id: 'admin/fedex-shipping.kg'})}</option>
            <option value='KG'>{formatMessage({id: 'admin/fedex-shipping.lb'})}</option>
          </Select>
          <Select
            label={formatMessage({id: 'admin/fedex-shipping.dimensions'})}
            value={unitDimension ?? 'IN'}
            onChange={(e) => setState({...state, unitDimension: e.target.value})}
          >
            <option value='IN'>{formatMessage({id: 'admin/fedex-shipping.in'})}</option>
            <option value='CM'>{formatMessage({id: 'admin/fedex-shipping.cm'})}</option>
          </Select>
        </Set>
        <Heading className='pt6'>{formatMessage({id: 'admin/fedex-shipping.hiddenSLA'})}</Heading>
        <Set orientation='vertical' spacing={3}>
          {generateCheckboxGroup()}
        </Set>
        <Set orientation='vertical' spacing={3}>
          <Heading className='pt6'>{formatMessage({id: 'admin/fedex-shipping.modalMap'})}</Heading>
          <Text variant='body'>{formatMessage({id: 'admin/fedex-shipping.modalMap.description'})}</Text>
          {generateModalMapping()}
        </Set>
      </div>
      <Button variant='primary' onClick={() => handleSave()}>{formatMessage({id: 'admin/fedex-shipping.saveSettings'})}</Button>
    </PageContent>
    )
    
}

export default Configurations
