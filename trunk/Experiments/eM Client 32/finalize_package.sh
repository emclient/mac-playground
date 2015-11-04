#
#pwd
#set

#CONTENTS_FOLDER_PATH=Test32.app/Contents
#TARGET_BUILD_DIR=/Users/jirkav/Library/Developer/Xcode/DerivedData/FakeMailApp-ddewicpaqkqskqblkgmblqmtkdpa/Build/Products/Debug

export LIB="$TARGET_BUILD_DIR/$CONTENTS_FOLDER_PATH/lib"
export DLL="$LIB/mono/4.5"
export GAC="$LIB/mono/gac"

# --- common  ------------------------------------------------------------------------------------------------------------


# --- formstest ----------------------------------------------------------------------------------------------------------

export APP="$LIB/formstest/Mono - Debug"

cp "$APP/System.Windows.Forms.dll" "$GAC/System.Windows.Forms/4.0.0.0__b77a5c561934e089/System.Windows.Forms.dll"
cp "$APP/System.Windows.Forms.dll.mdb" "$GAC/System.Windows.Forms/4.0.0.0__b77a5c561934e089/System.Windows.Forms.dll.mdb"
#rm "$GAC/System.Windows.Forms/4.0.0.0__b77a5c561934e089/System.Windows.Forms.dll" 2> /dev/null
#rm "$GAC/System.Windows.Forms/4.0.0.0__b77a5c561934e089/System.Windows.Forms.dll.mdb" 2> /dev/null
#cp "$APP/System.Windows.Forms.dll" "$LIB/System.Windows.Forms.dll"
#cp "$APP/System.Windows.Forms.dll.mdb" "$LIB/System.Windows.Forms.dll.mdb"
#rm "$APP/System.Windows.Forms.dll" 2> /dev/null
#rm "$APP/System.Windows.Forms.dll.mdb" 2> /dev/null

cp "$APP/System.Drawing.dll" "$GAC/System.Drawing/4.0.0.0__b03f5f7f11d50a3a/System.Drawing.dll"
cp "$APP/System.Drawing.dll.mdb" "$GAC/System.Drawing/4.0.0.0__b03f5f7f11d50a3a/System.Drawing.dll.mdb"
cp "$APP/System.Drawing.dll" "$LIB/System.Drawing.dll" 2> /dev/null
cp "$APP/System.Drawing.dll.mdb" "$LIB/System.Drawing.dll.mdb"2> /dev/null

#rm "$GAC/System.Drawing/4.0.0.0__b03f5f7f11d50a3a/System.Drawing.dll" 2> /dev/null
#rm "$GAC/System.Drawing/4.0.0.0__b03f5f7f11d50a3a/System.Drawing.dll.mdb" 2> /dev/null
#rm "$APP/System.Drawing.dll" 2> /dev/null
#rm "$APP/System.Drawing.dll.mdb" 2> /dev/null

# Bez tohoto nelze: MonoMac je referencovan z nasich Forms
cp "$APP/MonoMac.dll" "$LIB/MonoMac.dll"
cp "$APP/MacBridge.dll" "$LIB/MacBridge.dll"
cp "$APP/MailClient.Collections.dll" "$LIB/MailClient.Collections.dll"
#cp "$APP/System.Drawing.dll" "$LIB/System.Drawing.dll"
#cp "$APP/System.Drawing.dll.mdb" "$LIB/System.Drawing.dll.mdb"
#cp "$APP/System.Windows.Forms.dll" "$LIB/System.Windows.Forms.dll"
#cp "$APP/System.Windows.Forms.dll.mdb" "$LIB/System.Windows.Forms.dll.mdb"

#cp -R "${APP}"/* "${LIB}/"

# --- emclient -----------------------------------------------------------------------------------------------------------

export APP="$LIB/emclient/Mono - Debug"

#cp "$APP/System.Windows.Forms.dll" "$GAC/System.Windows.Forms/4.0.0.0__b77a5c561934e089/System.Windows.Forms.dll"
#cp "$APP/System.Windows.Forms.dll.mdb" "$GAC/System.Windows.Forms/4.0.0.0__b77a5c561934e089/System.Windows.Forms.dll.mdb"

#cp "$APP/System.Drawing.dll" "$GAC/System.Drawing/4.0.0.0__b03f5f7f11d50a3a/System.Drawing.dll"
#cp "$APP/System.Drawing.dll.mdb" "$GAC/System.Drawing/4.0.0.0__b03f5f7f11d50a3a/System.Drawing.dll.mdb"

#cp -R "${APP}"/* "${LIB}/"


# --- scrapbook  ---------------------------------------------------------------------------------------------------------

# --- Older attempts to replace Forms and Drawing using symlink evaluation
#export SYM=$(readlink "$DLL/System.Windows.Forms.dll")
#echo "$SYM"
#rm "$DLL/$SYM"
#cp "$APP/System.Windows.Forms.dll" "$DLL/$SYM"
#rm "$DLL/System.Windows.Forms.dll"
#cp "$APP/System.Windows.Forms.dll" "$LIB/System.Windows.Forms.dll"

#export SYM=$(readlink "$DLL/System.Drawing.dll")
#rm "$DLL/$SYM"
#cp "$APP/System.Drawing.dll" "$DLL/$SYM"
#rm "$DLL/System.Drawing.dll"
#cp "$APP/System.Drawing.dll" "$LIB/System.Drawing.dll"

