/* eslint-disable @typescript-eslint/no-explicit-any */
import type { FC } from 'react'
import React, { useState, useEffect } from 'react'
import { useIntl } from 'react-intl'
import {
  PageContent,
  Input,
  InputPassword,
  Toggle,
  Button,
  useToast,
  Set,
  Label,
  Tabs,
  TabPanel,
  Tab,
  TabList,
  IconGearSix,
  IconFaders,
  IconStorefront,
  Alert,
  Select,
  useTabState as UseTabState,
} from '@vtex/admin-ui'
import { useQuery, useMutation } from 'react-apollo'

import AppSettings from '../queries/getAppSettings.gql'
import SaveAppSetting from '../mutations/saveAppSetting.gql'
import DockConfig from './DockConfig'
import AdvanceConfigurations from './AdvanceConfigurations'
import { packingOptimization } from '../utils/constants'

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
    optimizeShippingType: number
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
    optimizeShippingType: 0,
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
    optimizeShippingType,
    unitWeight,
    unitDimension,
    itemModals: items,
    slaSettings,
  } = state

  const [saveAppSetting] = useMutation(SaveAppSetting)

  const tabState = UseTabState({ selectedId: '1' })

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

  const handleSetAdvanceSettings = (fieldName: string, newProp: any) => {
    setState({ ...state, [fieldName]: newProp })
  }

  const showToast = useToast()

  const handleSave = (saveModals: any, saveSlaSettings: any) => {
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
          optimizeShippingType,
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

  const mapAndSave = () => {
    const saveModals: any[] = []

    const saveSlaSettings: SlaSetting[] = []

    items.forEach((item) => {
      saveModals.push({
        modal: item.modal,
        fedexHandling: item.fedexHandling,
        shipAlone: item.shipAlone,
      })
    })

    slaSettings.forEach((sla) => {
      saveSlaSettings.push({
        sla: sla.sla,
        hidden: sla.hidden,
        surchargePercent: sla.surchargePercent,
        surchargeFlatRate: sla.surchargeFlatRate,
      })
    })

    handleSave(saveModals, saveSlaSettings)
  }

  const generateOptions = () => {
    const packingOptions = []

    for (const pack of packingOptimization) {
      packingOptions.push(<option value={pack}>{pack}</option>)
    }

    return packingOptions
  }

  return (
    <PageContent>
      <Tabs state={tabState}>
        <TabList fluid aria-label="fluid-tabs">
          <Tab id="1" csx={{ fontSize: '1.25rem' }}>
            <IconGearSix size="small" />
            {formatMessage({ id: 'admin/fedex-shipping.appSettings' })}
          </Tab>
          <Tab id="2" csx={{ fontSize: '1.25rem' }}>
            <IconFaders size="small" />
            {formatMessage({ id: 'admin/fedex-shipping.advanceConfig' })}
          </Tab>
          <Tab id="3" csx={{ fontSize: '1.25rem' }}>
            <IconStorefront size="small" />
            {formatMessage({ id: 'admin/fedex-shipping.dockConfig' })}
          </Tab>
        </TabList>
        <TabPanel id="1" csx={{ padding: 3 }}>
          <Set orientation="vertical" spacing={2} className="pb6">
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
                setState({
                  ...state,
                  clientDetailAccountNumber: e.target.value,
                })
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
              <Label>
                {formatMessage({ id: 'admin/fedex-shipping.isLive' })}
              </Label>
            </Set>
            <Set orientation="horizontal" spacing={2}>
              <Toggle
                aria-label="label"
                checked={residential}
                onChange={() =>
                  setState({ ...state, residential: !residential })
                }
              />
              <Label>
                {formatMessage({ id: 'admin/fedex-shipping.residential' })}
              </Label>
            </Set>
            <Set orientation="vertical" spacing={2}>
              <Select
                label={formatMessage({
                  id: 'admin/fedex-shipping.optimizeShipping',
                })}
                value={packingOptimization[optimizeShippingType]}
                onChange={(e) =>
                  setState({
                    ...state,
                    optimizeShippingType: packingOptimization.indexOf(
                      e.target.value
                    ),
                  })
                }
              >
                {generateOptions()}
              </Select>
              {optimizeShippingType === 2 ? (
                <Alert visible tone="warning">
                  {formatMessage({ id: 'admin/fedex-shipping.smartPackAlert' })}
                </Alert>
              ) : null}
            </Set>
          </Set>
          <Button variant="primary" onClick={() => mapAndSave()}>
            {formatMessage({ id: 'admin/fedex-shipping.saveSettings' })}
          </Button>
        </TabPanel>
        <TabPanel id="2" csx={{ padding: 3 }}>
          <AdvanceConfigurations
            handleAdvanceSave={handleSave}
            setChange={handleSetAdvanceSettings}
            unitDimension={unitDimension}
            unitWeight={unitWeight}
            slaSettings={slaSettings}
            items={items}
          />
        </TabPanel>
        <TabPanel id="3" csx={{ padding: 3 }}>
          <DockConfig />
        </TabPanel>
      </Tabs>
    </PageContent>
  )
}

export default Configurations
