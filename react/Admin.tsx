import type { FC } from 'react'
import React, { useState, useEffect } from 'react'
import { useIntl } from 'react-intl'
import {
  Page,
  PageHeader,
  PageTitle,
  PageContent,
  Input,
  InputPassword,
  Heading,
  createSystem,
  Toggle,
  Button
} from '@vtex/admin-ui'
import { useQuery, useMutation } from 'react-apollo'
import AppSettings from './queries/getAppSettings.gql'
import SaveAppSetting from './mutations/saveAppSetting.gql'


const [ThemeProvider] = createSystem({
  key: 'fedex-shipping',
})

const Admin: FC = () => {
  const { formatMessage } = useIntl()

  const { data } = useQuery(AppSettings, {
    ssr: false,
  })
  
  const [state, setState] = useState<any>({
    clientDetailMeterNumber: '',
    clientDetailAccountNumber: '',
    userCredentialKey: '',
    userCredentialPassword: '',
    isLive: false
  })

  const [saveAppSetting, { data: saveData, called, error }] = useMutation(SaveAppSetting);

  useEffect(() => {
    if (!data?.getAppSettings) return

    const getAppSettings = data.getAppSettings
    setState({...getAppSettings})
  }, [data])

  console.log(saveData, called, error)
  const { clientDetailMeterNumber, clientDetailAccountNumber, userCredentialKey, userCredentialPassword, isLive } = state

  const handleSave = () => {
    console.log({
      userCredentialKey,
      userCredentialPassword,
      parentCredentialKey: '',
      parentCredentialPassword: '',
      clientDetailMeterNumber,
      clientDetailAccountNumber,
      isLive: false
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
          isLive
        }
      }
    })
  }


  console.log(state)
  return (
    <ThemeProvider>
      <Page className="pa7">
        <PageHeader>
          <PageTitle>
            {formatMessage({id: 'admin/fedex-shipping.title'})}
          </PageTitle>
        </PageHeader>
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
            <Button variant="primary" onClick={() => handleSave()}>Save</Button>
          </div>
        </PageContent>
      </Page>
    </ThemeProvider>
    )
    
}

export default Admin
