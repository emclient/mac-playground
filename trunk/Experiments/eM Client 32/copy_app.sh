export BIN_DIR="$PROJECT_DIR/../../../bin/x86/Mono - Debug"
export OUT_DIR="$TARGET_BUILD_DIR/$CONTENTS_FOLDER_PATH/lib/emclient"

mkdir -p "$TARGET_BUILD_DIR/$CONTENTS_FOLDER_PATH/lib/emclient"
cp -r "$BIN_DIR/"* "$OUT_DIR"

cp "$BIN_DIR/"libhunspell.dylib "$TARGET_BUILD_DIR/$CONTENTS_FOLDER_PATH"

#export BIN_DIR="$PROJECT_DIR/../ConsoleTest/bin/Debug"
#export TMP_DIR="$PROJECT_DIR/../../../bin/x86/tmp"
#export OUT_DIR="$TARGET_BUILD_DIR/$CONTENTS_FOLDER_PATH/lib/emclient"

#cp -r "$BIN_DIR/" "$TMP_DIR/"
#mono --aot -O=all "$TMP_DIR/Google.Apis.dll"
#mono --aot=full "$TMP_DIR/MailClient.exe"
#rm "$TMP_DIR/MailClient.exe"
#mv "$TMP_DIR/MailClient.exe.config" "$TMP_DIR/MailClient.exe.dylib.config"
