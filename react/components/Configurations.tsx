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
} from '@vtex/admin-ui'
import { useQuery, useMutation } from 'react-apollo'
import AppSettings from '../queries/getAppSettings.gql'
import SaveAppSetting from '../mutations/saveAppSetting.gql'

const Configurations: FC = () => {
  const { formatMessage } = useIntl()

  const { data } = useQuery(AppSettings, {
    ssr: false,
  })
  
  const [state, setState] = useState<any>({
    clientDetailMeterNumber: '',
    clientDetailAccountNumber: '',
    userCredentialKey: '',
    userCredentialPassword: '',
    isLive: false,
    unitWeight: 'LB',
    unitDimension: 'IN',
  })

  const [saveAppSetting] = useMutation(SaveAppSetting);

  useEffect(() => {
    if (!data?.getAppSettings) return

    const getAppSettings = data.getAppSettings
    setState({...getAppSettings})
  }, [data])

  const showToast = useToast()

  const { clientDetailMeterNumber, clientDetailAccountNumber, userCredentialKey, userCredentialPassword, isLive, unitWeight, unitDimension } = state

  const handleSave = () => {
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
          unitWeight,
          unitDimension
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


  return (
    <PageContent>
      <div className="pv6">
        <Heading>{formatMessage({id: 'admin/fedex-shipping.settings'})}</Heading>
        <Input id="meter" label={formatMessage({id: 'admin/fedex-shipping.meter'})} value={clientDetailMeterNumber} 
          onChange={(e) => setState({...state, clientDetailMeterNumber: e.target.value})}
          />
        <Input id="accountNumber" label={formatMessage({id: 'admin/fedex-shipping.accountNum'})} value={clientDetailAccountNumber}
          onChange={(e) => setState({...state, clientDetailAccountNumber: e.target.value})}
          />
        <InputPassword id="credentialKey" label={formatMessage({id: 'admin/fedex-shipping.credKey'})} value={userCredentialKey}
          onChange={(e) => setState({...state, userCredentialKey: e.target.value})}
          />
        <InputPassword id="credentialPwd" label={formatMessage({id: 'admin/fedex-shipping.credPwd'})} value={userCredentialPassword}
          onChange={(e) => setState({...state, userCredentialPassword: e.target.value})}
        />
        <div>
        {formatMessage({id: 'admin/fedex-shipping.isLive'})}<Toggle
            aria-label="label"
            checked={isLive}
            onChange={() => setState({...state, isLive: !isLive})}
          />
        </div>
        <Set spacing={3}>
          <Select
            label={formatMessage({id: 'admin/fedex-shipping.weight'})}
            value={unitWeight ?? "LB"}
            onChange={(e) => setState({...state, unitWeight: e.target.value})}
          >
            <option value="LB">{formatMessage({id: 'admin/fedex-shipping.kg'})}</option>
            <option value="KG">{formatMessage({id: 'admin/fedex-shipping.lb'})}</option>
          </Select>
          <Select
            label={formatMessage({id: 'admin/fedex-shipping.dimensions'})}
            value={unitDimension ?? "IN"}
            onChange={(e) => setState({...state, unitDimension: e.target.value})}
          >
            <option value="IN">{formatMessage({id: 'admin/fedex-shipping.in'})}</option>
            <option value="CM">{formatMessage({id: 'admin/fedex-shipping.cm'})}</option>
          </Select>
        </Set>
      </div>
      <Button variant="primary" onClick={() => handleSave()}>{formatMessage({id: 'admin/fedex-shipping.saveSettings'})}</Button>
    </PageContent>
    )
    
}

export default Configurations
