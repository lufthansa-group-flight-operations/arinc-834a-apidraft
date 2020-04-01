//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// AvionicParameterView takes a parameter from backend and visualizes it as a list item with icon depending on the paramters state

struct AvionicParameterView: View {
    
    var parameter: AvionicParameter
    
    var body: some View {
        HStack {
            getStateIcon(state: parameter.state)
            Text(parameter.name)
            Spacer()
            Text(parameter.value)
            Spacer()
            Text(parameter.timestamp)
        }
    }
    
    func getStateIcon(state: String) -> some View {
        switch state {
            case "0":
                return Image(systemName: "n.square").foregroundColor(.green)
            case "1":
                return Image(systemName: "f.square").foregroundColor(.yellow)
            case "2":
                return Image(systemName: "f.square").foregroundColor(.red)
            default:
                return Image(systemName: "f.square").foregroundColor(.red)            
        }
    }
}

struct AvionicParameterView_Previews: PreviewProvider {
    static var previews: some View {
        AvionicParameterView(parameter: AvionicParameter(id: UUID(),name: "Test",value: "TestValue",timestamp: "TestTime",state: "STATE"))
    }
}
