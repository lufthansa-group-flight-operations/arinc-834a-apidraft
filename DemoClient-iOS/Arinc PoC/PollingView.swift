//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import SwiftUI

// The view visualizes the avionic parameters collected from the PollingService via REST GET command
struct PollingView: View {
    @EnvironmentObject var pollingClient : PollingService
    
    @State private var serverAdress: String = "https://server:port"

    var body: some View {
        VStack(alignment: HorizontalAlignment.leading) {
            VStack {
                Text("REST Polling").font(.title)
            }
            HStack {
                TextField("https://server:port",text: self.$serverAdress).background(Color.white.shadow(radius: 2)).foregroundColor(Color.black)
                Button(action:{self.pollingClient.startPolling(address: self.serverAdress, deltaT: 1.0)}){Text("Start")}
                Button(action:{self.pollingClient.stopPolling()}){Text("Stop")}
            }
            List(self.pollingClient.pollingResults) { parameter in
                AvionicParameterView(parameter: parameter)
            }
        }
    }
}

struct PollingView_Previews: PreviewProvider {
    static var previews: some View {
        PollingView()
    }
}
