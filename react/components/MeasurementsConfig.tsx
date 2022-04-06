import type { FC } from 'react'
import React, { useState } from 'react'
import { Select, Set, PageContent, Button } from '@vtex/admin-ui'

const MeasurementsConfig: FC = () => {

  const [state, setState] = useState<any>({
    unitWeight: 'LB',
    unitDimensions: 'IN',
  })

  const { unitWeight, unitDimensions } = state

  const handleSave = () => {
    console.log(state)
  }

  return (
    <PageContent>
      <Set spacing={3}>
        <Select
          label="Weight"
          value={unitWeight}
          onChange={(e) => setState({...state, unitWeight: e.target.value})}
        >
          <option value="LB">Pounds</option>
          <option value="KG">Kilograms</option>
        </Select>
        <Select
          label="Dimensions"
          value={unitDimensions}
          onChange={(e) => setState({...state, unitDimensions: e.target.value})}
        >
          <option value="IN">Inches</option>
          <option value="CM">Centimeters</option>
        </Select>
        <Button onClick={() => handleSave()}>Save Unit of Measurement</Button>
      </Set>
    </PageContent>
  )
}

export default MeasurementsConfig