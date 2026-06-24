// AUTO-GENERATED FILE. Do not edit directly.
// Run the component registry generator to regenerate.

import { register } from './registry'
import FrontendTheme from '@theme:frontend/Default'
import AdminTheme from '@theme:admin/Default'
export { FrontendTheme, AdminTheme } 

import { FlashCards } from '@component/Pages/FlashCards'
import { Study } from '@component/Pages/FlashCards/Study'
import { LanguageSelector } from '@component/LanguageSelector'
import { Text } from '@component/Text'
import { Spinner } from '@component/Spinner'

register('@component/Pages/FlashCards', FlashCards)
register('@component/Pages/FlashCards/Study', Study)
register('@component/LanguageSelector', LanguageSelector)
register('@component/Text', Text)
register('@component/Spinner', Spinner)

// auto imports
import '../../../../../.././App/Code/Local/Scenarios/Source/web/Pages/Scenarios/index.tsx';
import '../../../../../.././App/Code/Local/Scenarios/Source/web/Pages/ScenarioSession/index.tsx';
import '../../../../../.././App/Code/Community/MusicTranslation/Source/web/TranslationPage/index.tsx';
