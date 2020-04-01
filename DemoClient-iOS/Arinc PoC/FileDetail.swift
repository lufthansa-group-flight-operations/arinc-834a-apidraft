//
// Copyright (c) Deutsche Lufthansa AG.
// Author Frank Hundrieser
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

import Foundation

// FileDetail holds file meta data and the id so it can be identified

struct FileDetail: Identifiable {
    var id = UUID()
    var name: String
    var size: String
    var lastChange: String
}
