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
  useTabState as UseTabState,
} from '@vtex/admin-ui'
import { useQuery, useMutation } from 'react-apollo'

import AppSettings from '../queries/getAppSettings.gql'
import SaveAppSetting from '../mutations/saveAppSetting.gql'
import DockConfig from './DockConfig'
import AdvanceConfigurations from './AdvanceConfigurations'

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

  const handleSetDimensions = (unitDimensionInput: string) => {
    setState({ ...state, unitDimension: unitDimensionInput })
  }

  const handleSetUnitWeights = (unitWeightInput: string) => {
    setState({ ...state, unitWeight: unitWeightInput })
  }

  const handleSetSurcharge = (slaSettingsInput: any[]) => {
    setState({ ...state, slaSettings: slaSettingsInput })
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

  return (
    <PageContent>
      <Tabs state={tabState}>
        <TabList fluid aria-label="fluid-tabs">
          <Tab id="1" csx={{ fontSize: '1.25rem' }}>
            <IconGearSix size="small" />
            App Settings
          </Tab>
          <Tab id="2" csx={{ fontSize: '1.25rem' }}>
            <IconFaders size="small" />
            Advance Configurations
          </Tab>
          <Tab id="3" csx={{ fontSize: '1.25rem' }}>
            <IconStorefront size="small" />
            Dock Configurations
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
          <Button variant="primary" onClick={() => mapAndSave()}>
            {formatMessage({ id: 'admin/fedex-shipping.saveSettings' })}
          </Button>
        </TabPanel>
        <TabPanel id="2" csx={{ padding: 3 }}>
          <AdvanceConfigurations
            setDimensions={handleSetDimensions}
            setUnitWeights={handleSetUnitWeights}
            handleAdvanceSave={handleSave}
            setSlaSurcharge={handleSetSurcharge}
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
