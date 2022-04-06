import type { FC } from 'react'
import React, { useState, useEffect } from 'react'
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
        <Heading>Settings</Heading>
        <Input id="meter" label="Meter Number" value={clientDetailMeterNumber} 
          onChange={(e) => setState({...state, clientDetailMeterNumber: e.target.value})}
          />
        <Input id="accountNumber" label="Account Number" value={clientDetailAccountNumber}
          onChange={(e) => setState({...state, clientDetailAccountNumber: e.target.value})}
          />
        <InputPassword id="credentialKey" label="Credential Key" value={userCredentialKey}
          onChange={(e) => setState({...state, userCredentialKey: e.target.value})}
          />
        <InputPassword id="credentialPwd" label="Credential Password" value={userCredentialPassword}
          onChange={(e) => setState({...state, userCredentialPassword: e.target.value})}
        />
        <div>
          Is Live: <Toggle
            aria-label="label"
            checked={isLive}
            onChange={() => setState({...state, isLive: !isLive})}
          />
        </div>
        <Set spacing={3}>
          <Select
            label="Weight"
            value={unitWeight ?? "LB"}
            onChange={(e) => setState({...state, unitWeight: e.target.value})}
          >
            <option value="LB">Pounds</option>
            <option value="KG">Kilograms</option>
          </Select>
          <Select
            label="Dimensions"
            value={unitDimension ?? "IN"}
            onChange={(e) => setState({...state, unitDimension: e.target.value})}
          >
            <option value="IN">Inches</option>
            <option value="CM">Centimeters</option>
          </Select>
        </Set>
      </div>
      <Button variant="primary" onClick={() => handleSave()}>Save</Button>
    </PageContent>
    )
    
}

export default Configurations
