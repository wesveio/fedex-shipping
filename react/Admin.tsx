import type { FC } from 'react'
import React from 'react'
import { useIntl } from 'react-intl'
import {
  Page,
  PageHeader,
  PageTitle,
  createSystem,
  ToastProvider,
} from '@vtex/admin-ui'
import Configurations from './components/Configurations'

const Admin: FC = () => {
  const [ThemeProvider] = createSystem({
    key: 'fedex-shipping',
  })

  const { formatMessage } = useIntl()

  return (
    <ThemeProvider>
      <ToastProvider>
        <Page className="pa7">
          <PageHeader>
            <PageTitle>
              {formatMessage({id: 'admin/fedex-shipping.title'})}
            </PageTitle>
          </PageHeader>
          <Configurations/>
        </Page>
      </ToastProvider>
    </ThemeProvider>
    )
    
}

export default Admin
