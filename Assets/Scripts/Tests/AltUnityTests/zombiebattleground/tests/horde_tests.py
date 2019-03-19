import unittest

import xmlrunner

from pages.base import CZBTests
from pages.deck_rename_tab_page import Deck_Rename_Tab_Page
from pages.deck_selection_page import Deck_Selection_Page
from pages.horde_editing_page import Horde_Editing_Page
from pages.overlord_ability_popup_page import Overlord_Ability_Popup_Page
from pages.overlord_selection_page import Overlord_Selection_Page
from pages.side_menu_popup_page import Side_Menu_Popup_Page


class CZBHordeTests(CZBTests):


    def setUp(self):
        super(CZBHordeTests, self).setUp()
        self.skip_tutorials()

    def test_create_new_deck(self):
        Side_Menu_Popup_Page(self.altdriver).press_my_decks_button()
        Deck_Selection_Page(self.altdriver).create_new_deck()
        Overlord_Selection_Page(self.altdriver).press_continue()
        Overlord_Ability_Popup_Page(self.altdriver).press_continue()
        Deck_Rename_Tab_Page(self.altdriver).press_save_button()       
        horde_editing_page=Horde_Editing_Page(self.altdriver,self.driver)
        horde_editing_page.add_cards_to_horde(15)
        horde_editing_page.press_save()
        


    # def test_default_Horde_available(self):
    #     default_deck=self.get_default_deck()
    #     deck_name=self.altdriver.wait_for_element('id('+str(default_deck.id)+')/Panel_Description/Text_Description').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro')
    #     self.assertEqual(deck_name,'HORDE 0')
    
    
    # def test_create_new_deck(self):
    #     self.create_new_deck()
    #     self.altdriver.wait_for_element(('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)'))
    #     decks=self.altdriver.find_elements('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)')
    #     self.assertEqual(len(decks),2)
    
    
    # def test_edit_deck(self):
    #     self.create_new_deck()
    #     custom_deck=self.get_custom_deck()
    #     deck_name=self.altdriver.wait_for_element('id('+str(self.get_custom_deck().id)+')/Panel_Description/Text_Description').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro')
    #     deck_number_of_cards=self.altdriver.wait_for_element('id('+str(self.get_custom_deck().id)+')/Panel_DeckFillInfo/Text_CardsCount').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro')
    #     self.edit_deck(custom_deck)
    #     self.add_card_to_deck(1)
    #     self.change_deck_name(custom_deck,"testTitle")
    #     self.save_deck()
        
    #     new_deck_name=self.altdriver.wait_for_element('id('+str(self.get_custom_deck().id)+')/Panel_Description/Text_Description').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro')
    #     new_deck_number_of_cards=self.altdriver.wait_for_element('id('+str(self.get_custom_deck().id)+')/Panel_DeckFillInfo/Text_CardsCount').get_component_property('TMPro.TextMeshProUGUI','text','Unity.TextMeshPro')

    #     # self.assertNotEqual(deck_name,new_deck_name)
    #     self.assertNotEqual(new_deck_number_of_cards,deck_number_of_cards)
    
    # def test_adding_more_30_card(self):
    #     self.create_new_deck()
    #     custom_deck=self.get_custom_deck()
    #     name=self.altdriver.wait_for_element('id('+custom_deck.id+')/Panel_Description/Text_Description').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
    #     self.edit_deck(custom_deck)
    #     self.add_card_to_deck(31)
    #     warning_message=self.altdriver.wait_for_element('WarningPopup(Clone)/Text_Message').get_component_property('TMPro.TextMeshProUGUI', 'text', 'Unity.TextMeshPro')
    #     self.assertEqual(warning_message,'Your \''+name+'\' deck has more than 30 cards.')
    #     self.altdriver.wait_for_element('Button_GotIt').mobile_tap()
    #     self.save_deck()
    #     self.delete_existing_custom_deck()


    # def test_check_all_cards(self):
    #     army_cards=[]
    #     self.altdriver.wait_for_element('HordeSelectionPage(Clone)')
    #     time.sleep(2)
    #     self.altdriver.wait_for_element('Item_HordeSelectionNewHorde/Image_BaackgroundGeneral').mobile_tap()
    #     self.altdriver.wait_for_element('OverlordSelectionPage(Clone)/Image_BottomMask/Button_Continue').mobile_tap()
    #     self.altdriver.wait_for_element('OverlordAbilityPopup(Clone)/Canvas_BackLayer/Button_Continue').mobile_tap()
    #     self.altdriver.wait_for_element('HordeEditingPage(Clone)')

    #     gone_through_all_the_cards=False
    #     while gone_through_all_the_cards==False:
    #         cards_on_page=self.altdriver.find_elements('Army/Cards/CreatureCard(Clone)/TitleText')
    #         if cards_on_page==[]:
    #             cards_on_page=self.altdriver.find_elements('Army/Cards/ItemCard(Clone)/TitleText')
    #         for card in cards_on_page:
    #             card_name=card.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro')
    #             if self.check_name_is_in_list(army_cards,card_name):
    #                 gone_through_all_the_cards=True
    #                 break
    #             else:
    #                 army_cards.append(card_name)

                   
    #         self.altdriver.wait_for_element('Army/ArrowRightButton').mobile_tap()
    #         time.sleep(1)
    #     print(len(army_cards))
    #     self.assertEqual(len(army_cards),101)


    # def add_card_to_deck(self,number_of_cards):
    #     horde=self.altdriver.wait_for_element('Horde/ScrollArea')
       
    #     counter=0
    #     while counter!=number_of_cards:
    #         card_from_where_to_draw=self.altdriver.find_elements('Army/Cards/CreatureCard(Clone)/AmountForArmy/Text')
    #         for i in card_from_where_to_draw:
    #             number_of_available_cards=int(i.get_component_property('TMPro.TextMeshPro', 'text', 'Unity.TextMeshPro'))
    #             if number_of_available_cards+counter>number_of_cards:
    #                 number_of_available_cards=number_of_cards-counter
    #             if number_of_available_cards>0:
    #                 card=self.altdriver.find_element('id('+str(i.id)+')/../..')
    #                 for j in range(number_of_available_cards):
    #                     self.driver.swipe(card.x,card.mobileY,horde.x,horde.mobileY,2000)
    #                     counter=counter+1
    #                     if counter==number_of_cards:
    #                         break

    #         self.altdriver.wait_for_element('Army/ArrowRightButton').mobile_tap()
    #         time.sleep(1)


    # def create_new_deck(self):
    #    self.altdriver.wait_for_element('HordeSelectionPage(Clone)')
    #    time.sleep(2)
    #    self.altdriver.wait_for_element('Item_HordeSelectionNewHorde/Image_BaackgroundGeneral').mobile_tap()
    #    self.altdriver.wait_for_element('OverlordSelectionPage(Clone)/Image_BottomMask/Button_Continue').mobile_tap()
    #    self.altdriver.wait_for_element('OverlordAbilityPopup(Clone)/Canvas_BackLayer/Button_Continue').mobile_tap()
    #    self.altdriver.wait_for_element('HordeEditingPage(Clone)')
    #    self.altdriver.wait_for_element('HordeEditingPage(Clone)/Button_Save').mobile_tap()


    # def edit_deck(self,deck):
    #     self.altdriver.wait_for_element('HordeSelectionPage(Clone)')
    #     time.sleep(2)
    #     self.altdriver.wait_for_element('id('+str(deck.id)+')').mobile_tap()
    #     self.altdriver.wait_for_element('Button_Edit').mobile_tap()
        

    # def save_deck(self):
    #     self.altdriver.wait_for_element('HordeEditingPage(Clone)/Button_Save').mobile_tap()
    #     time.sleep(2)



    # def change_deck_name(self,customDeck,deckName):
    #     self.altdriver.wait_for_element('DeckTitleInputText').set_component_property('TMPro.TMP_InputField','text',deckName,'Unity.TextMeshPro')
    #     # self.altdriver.wait_for_element('DeckTitleInputText').mobile_tap()


    # def get_custom_deck(self):
    #     self.altdriver.wait_for_element(('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)'))
    #     decks=self.altdriver.find_elements('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)')
    #     self.assertEqual(len(decks),2)
    #     if int(decks[0].x)<int(decks[1].x):
    #         return decks[1]
    #     else:
    #         return decks[0]

    # def get_default_deck(self):
    #     self.altdriver.wait_for_element(('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)'))
    #     decks=self.altdriver.find_elements('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)')
    #     self.assertEqual(len(decks),2)
    #     if int(decks[0].x)>int(decks[1].x):
    #         return decks[1]
    #     else:
    #         return decks[0]


    # def delete_existing_custom_deck(self):
    #     self.altdriver.wait_for_element('HordeSelectionPage(Clone)')
    #     time.sleep(2)
       
    #     if len(self.altdriver.find_elements('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)'))>1:
    #         default_deck=self.get_default_deck()
    #         self.altdriver.wait_for_element('id('+str(default_deck.id)+')/Button_Select').tap()
    #         self.altdriver.wait_for_element('HordeSelectionPage(Clone)/Panel_DecksContainer/Group/Item_HordeSelectionObject(Clone)')
    #         custom_deck=self.get_custom_deck()
    #         self.altdriver.wait_for_element('id('+str(custom_deck.id)+')/Button_Select').tap()
    #         self.altdriver.wait_for_element('Button_Delete').mobile_tap()
    #         self.altdriver.wait_for_element('Button_Yes').mobile_tap()



    # def setUp(self):
    #     super(CZBHordeTests, self).setUp()
    #     self.pass_authentification()
    

    #     self.skip_both_tutorials()
    #     self.delete_existing_custom_deck()


if __name__ == '__main__':
   unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
